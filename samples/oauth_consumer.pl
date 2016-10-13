#!/usr/bin/perl
use strict;
use warnings;
use utf8;

use Mojolicious::Lite;
use OAuth::Lite::Consumer;
use OAuth::Lite::Token;
use JSON;

my $consumer_key = 'OjrD3wav+EZbSw==';
my $consumer_secret = 'yKV005kzISrG63/vk1l5mApZi8I=';

my $consumer = OAuth::Lite::Consumer->new(
    consumer_key       => $consumer_key,
    consumer_secret    => $consumer_secret,
    site               => q{https://www.hatena.com},
    request_token_path => q{/oauth/initiate},
    access_token_path  => q{/oauth/token},
    authorize_path     => q{https://www.hatena.ne.jp/oauth/authorize},
);


get '/' => sub {
    my $self = shift;
    $self->stash(access_token => $self->session('access_token') || '');
} => 'root';


# リクエストトークン取得から認証用URLにリダイレクトするためのアクション
get '/oauth' => sub {
    my $self = shift;
    $self->stash(consumer => $consumer);

    # リクエストトークンの取得
    my $request_token = $consumer->get_request_token(
        callback_url => q{http://localhost:3000/callback},
        scope        => 'read_public',
    ) or die $consumer->errstr;
    # セッションへリクエストトークンを保存しておく
    $self->session(request_token => $request_token->as_encoded);

    # 認証用URLにリダイレクトする
    $self->redirect_to( $consumer->url_to_authorize(
        token        => $request_token,
    ) );
};


# 認証からコールバックされ、アクセストークンを取得するためのアクション
get '/callback' => sub {
    my $self = shift;
    $self->stash(consumer => $consumer);
    my $verifier = $self->param('oauth_verifier');
    my $request_token = OAuth::Lite::Token->from_encoded($self->session('request_token'));

    # リクエストトークンとverifierなどを用いてアクセストークンを取得
    my $access_token = $consumer->get_access_token(
        token    => $request_token,
        verifier => $verifier,
    ) or die $consumer->errstr;

    $self->session(request_token => undef);

    # アクセストークンをセッションに記録しておく
    $self->session(access_token  => $access_token->as_encoded);

    $self->redirect_to('/');
} => 'callback';


# アクセストークンを利用して、OAuthに対応したAPIを利用するためのアクション
get '/hello' => sub {
    my $self = shift;
    $self->stash(consumer => $consumer);
    my $access_token = OAuth::Lite::Token->from_encoded($self->session('access_token')) or return;

    # access_tokenなどを使ってAPIにアクセスする
    my $res = $consumer->request(
        method => 'GET',
        url    => 'http://n.hatena.com/applications/my.json',
        token  => $access_token,
        params => {},
    ) or die $consumer->errstr;

    my $data = decode_json($res->decoded_content || $res->content);
    $self->stash(user_data => $data);
} => 'hello';

app->start;

__DATA__
@@ root.html.ep
<a href="/oauth">はてなOAuth認証をする</a>
<br />
<% if ($access_token) { %>
  <a href="/hello">Hello API</a>
<% } %>

@@ callback.html.ep
% my $token = $self->stash('token');
<%= $token->token %></br>
<%= $token->secret %>

@@ hello.html.ep
url_name : <%= $user_data->{url_name} %> </br>
display_name : <%= $user_data->{display_name} %>

@@ exception.html.ep
REQUEST ERROR: <%= $consumer->errstr %> </br>
WWW-Authenticate: <%= $consumer->oauth_res && $consumer->oauth_res->header('www-authenticate') %>

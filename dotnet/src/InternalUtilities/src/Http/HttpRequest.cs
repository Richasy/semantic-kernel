// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.SemanticKernel;

[ExcludeFromCodeCoverage]
internal static class HttpRequest
{
    private static readonly HttpMethod s_patchMethod = new("PATCH");

    public static HttpRequestMessage CreateGetRequest(string url, object? payload = null, JsonTypeInfo? typeInfo = null) =>
        CreateRequest(HttpMethod.Get, url, payload, typeInfo);

    public static HttpRequestMessage CreatePostRequest(string url, object? payload = null, JsonTypeInfo? typeInfo = null) =>
        CreateRequest(HttpMethod.Post, url, payload, typeInfo);

    public static HttpRequestMessage CreatePostRequest(Uri url, object? payload = null, JsonTypeInfo? typeInfo = null) =>
        CreateRequest(HttpMethod.Post, url, payload, typeInfo);

    public static HttpRequestMessage CreatePutRequest(string url, object? payload = null, JsonTypeInfo? typeInfo = null) =>
        CreateRequest(HttpMethod.Put, url, payload, typeInfo);

    public static HttpRequestMessage CreatePatchRequest(string url, object? payload = null, JsonTypeInfo? typeInfo = null) =>
        CreateRequest(s_patchMethod, url, payload, typeInfo);

    public static HttpRequestMessage CreateDeleteRequest(string url, object? payload = null, JsonTypeInfo? typeInfo = null) =>
        CreateRequest(HttpMethod.Delete, url, payload, typeInfo);

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, object? payload, JsonTypeInfo? typeInfo) =>
        new(method, url) { Content = CreateJsonContent(payload, typeInfo) };

    private static HttpRequestMessage CreateRequest(HttpMethod method, Uri url, object? payload, JsonTypeInfo? typeInfo) =>
        new(method, url) { Content = CreateJsonContent(payload, typeInfo) };

    private static HttpContent? CreateJsonContent(object? payload, JsonTypeInfo? typeInfo)
    {
        HttpContent? content = null;
        if (payload is not null)
        {
            byte[] utf8Bytes = payload is string s ?
                Encoding.UTF8.GetBytes(s) :
                JsonSerializer.SerializeToUtf8Bytes(payload, typeInfo!);

            content = new ByteArrayContent(utf8Bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
        }

        return content;
    }
}

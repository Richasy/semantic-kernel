﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Connectors.Anthropic.Core;

internal sealed class AuthorRoleConverter : JsonConverter<AuthorRole?>
{
    public override AuthorRole? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? role = reader.GetString();
        if (role == null)
        {
            return null;
        }

        if (role.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            return AuthorRole.User;
        }

        if (role.Equals("assistant", StringComparison.OrdinalIgnoreCase))
        {
            return AuthorRole.Assistant;
        }

        throw new JsonException($"Unexpected author role: {role}");
    }

    public override void Write(Utf8JsonWriter writer, AuthorRole? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value == AuthorRole.Assistant)
        {
            writer.WriteStringValue("assistant");
        }
        else if (value == AuthorRole.User)
        {
            writer.WriteStringValue("user");
        }
        else
        {
            throw new JsonException($"Anthropic API doesn't support author role: {value}");
        }
    }
}

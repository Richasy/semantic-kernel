// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

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

        if (role.Equals("tool", StringComparison.OrdinalIgnoreCase))
        {
            return AuthorRole.Tool;
        }

        if (role.Equals("system", StringComparison.OrdinalIgnoreCase))
        {
            return AuthorRole.System;
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

        if (value == AuthorRole.Tool)
        {
            writer.WriteStringValue("tool");
        }
        else if (value == AuthorRole.Assistant)
        {
            writer.WriteStringValue("assistant");
        }
        else if (value == AuthorRole.User)
        {
            writer.WriteStringValue("user");
        }
        else if (value == AuthorRole.System)
        {
            writer.WriteStringValue("system");
        }
        else
        {
            throw new JsonException($"Spark API doesn't support author role: {value}");
        }
    }
}

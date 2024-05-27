// Copyright (c) Microsoft. All rights reserved.

using System;
using Azure.Core;
using Azure.Core.Pipeline;

namespace Microsoft.SemanticKernel.Connectors.OpenAI.Core.AzureSdk;

internal sealed class CustomHostPipelinePolicy : HttpPipelineSynchronousPolicy
{
    private readonly Uri _endpoint;

    internal CustomHostPipelinePolicy(Uri endpoint)
    {
        this._endpoint = endpoint;
    }

    public override void OnSendingRequest(HttpMessage message)
    {
        // Update current host to provided endpoint
        var uriBuilder = message.Request?.Uri;
        if (uriBuilder != null)
        {
            uriBuilder.Host = this._endpoint.Host;
            uriBuilder.Port = this._endpoint.Port;
            uriBuilder.Path = uriBuilder.Path.Replace("v1", this._endpoint.AbsolutePath.Trim('/'));
        }
    }
}

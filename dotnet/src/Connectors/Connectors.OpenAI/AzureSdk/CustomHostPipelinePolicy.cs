// Copyright (c) Microsoft. All rights reserved.

using System;
using Azure.Core;
using Azure.Core.Pipeline;

namespace Microsoft.SemanticKernel.Connectors.OpenAI.Core.AzureSdk;

internal class CustomHostPipelinePolicy : HttpPipelineSynchronousPolicy
{
    private readonly Uri _endpoint;

    internal CustomHostPipelinePolicy(Uri endpoint)
    {
        this._endpoint = endpoint;
    }
    public override void OnSendingRequest(HttpMessage message)
    {
        if (message?.Request == null)
        {
            return;
        }

        // Update current host to provided endpoint
        var uri = message.Request.Uri.ToString();
        var newUrl = uri.Replace("https://api.openai.com/v1", this._endpoint.ToString().TrimEnd('/'));
        message.Request.Uri.Reset(new Uri(newUrl));
    }
}

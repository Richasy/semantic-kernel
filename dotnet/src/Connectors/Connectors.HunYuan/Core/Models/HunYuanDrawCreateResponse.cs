// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

internal sealed class HunYuanDrawCreateResponse
{
    public HunYuanDrawCreateResponseContent? Response { get; set; }

    internal sealed class HunYuanDrawCreateResponseContent
    {
        public string? JobId { get; set; }

        public string? RequestId { get; set; }
    }
}

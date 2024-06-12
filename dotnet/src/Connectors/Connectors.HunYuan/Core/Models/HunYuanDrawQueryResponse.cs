// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

internal sealed class HunYuanDrawQueryResponse
{
    public HunYuanDrawQueryResponseContent? Response { get; set; }

    internal sealed class HunYuanDrawQueryResponseContent
    {
        public string? JobStatusCode { get; set; }

        public string? JobStatusMsg { get; set; }

        public string? JobErrorCode { get; set; }

        public string? JobErrorMsg { get; set; }

        public IList<string>? ResultImage { get; set; }

        public IList<string>? RevisedPrompt { get; set; }

        public string? RequestId { get; set; }
    }
}

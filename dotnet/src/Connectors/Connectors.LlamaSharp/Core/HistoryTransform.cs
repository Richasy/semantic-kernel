// Copyright (c) Microsoft. All rights reserved.

using LLama.Common;
using static LLama.LLamaTransforms;

namespace Microsoft.SemanticKernel.Connectors.LlamaSharp.Core;

internal sealed class BasicHistoryTransform : DefaultHistoryTransform
{
    public override string HistoryToText(ChatHistory history)
    {
        return base.HistoryToText(history) + $"{AuthorRole.Assistant}: ";
    }
}

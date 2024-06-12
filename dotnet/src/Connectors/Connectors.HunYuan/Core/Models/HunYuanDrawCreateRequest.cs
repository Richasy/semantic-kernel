// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

internal sealed class HunYuanDrawCreateRequest
{
    public string? Prompt { get; set; }

    public string? Resolution { get; set; }

    public int LogoAdd { get; set; } = 1;

    public int Revise { get; set; } = 1;
}

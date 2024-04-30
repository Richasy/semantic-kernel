// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

internal record QianFanTokenContext(QianFanAuthToken Token, DateTime GenerationTime)
{
    public bool IsValid => this.GenerationTime.AddSeconds(this.Token.ExpiresIn - 60) < DateTime.Now;
}

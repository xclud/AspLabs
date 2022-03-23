// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Moq;
using Xunit;

namespace System.Web.Adapters.Playwright.Tests;

public class RequestTests
{
}

[CollectionDefinition("Playwright")]
public class PlaywriteFixtureCollection : ICollectionFixture<PlaywrightFixture>
{
}

public class PlaywrightFixture
{
    private readonly Uri? _aspNetFramework;

    public PlaywrightFixture()
    {
        Microsoft.Playwright.Program.Main(new[] { "install" });

        if (Environment.GetEnvironmentVariable("ADAPTER_FRAMEWORK_URL") is { } url)
        {
            _aspNetFramework = new(url);
        }

        if (Environment.GetEnvironmentVariable("ADAPTER_FRAMEWORK_URL") is { } url)
        {
            _aspNetFramework = new(url);
        }
    }

    public Uri AspNetFramework { get; }

    public Uri AspNetCore { get; }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Xml;
using System.Xml.Linq;
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
    }

    private string GetFrameworkUrl()
    {
        XNamespace NS = "http://schemas.microsoft.com/developer/msbuild/2003";
        var doc = XDocument.Load("settings/framework.xml");

        var node = doc.Descendants(NS + "DevelopmentServerPort");
    }

    public Uri AspNetFramework { get; }

    public Uri AspNetCore { get; }
}

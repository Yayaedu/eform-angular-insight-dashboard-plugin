/*
The MIT License (MIT)

Copyright (c) 2007 - 2021 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace InsightDashboard.Pn.Test.Helpers;

using System.Threading.Tasks;
using eFormCore;
using Microting.eForm.Infrastructure.Helpers;
using Microting.eFormApi.BasePn.Abstractions;
using NSubstitute;

// Løser det, der tidligere blokerede test af QuestionsService/OptionsService/
// QuestionSetsService/DeviceSyncService: de bruger alle IEFormCoreService.
// GetCore() -> Core.DbContextHelper.GetDbContext(). Core er ikke en
// interface man kan substitute'e, og Core.Start(...) forsøger at forbinde
// til Microtings rigtige cloud-infrastruktur, spawner baggrundstråde osv.,
// hvilket hverken er nødvendigt eller ønsket i en test.
//
// Løsningen: Core.DbContextHelper er et almindeligt public felt (ikke en
// interface-property), så vi kan bygge et "tomt" Core-objekt og pege dets
// DbContextHelper direkte på testdatabasen. Alle services i dette projekt
// rører udelukkende core.DbContextHelper.GetDbContext() — intet andet af
// Core bruges nogen steder i vores kode.
public static class InsightCoreTestHelper
{
    public static IEFormCoreService BuildFakeCoreService(string connectionString)
    {
        var core = new Core
        {
            DbContextHelper = new DbContextHelper(connectionString)
        };

        var coreHelper = Substitute.For<IEFormCoreService>();
        coreHelper.GetCore().Returns(Task.FromResult(core));
        return coreHelper;
    }
}

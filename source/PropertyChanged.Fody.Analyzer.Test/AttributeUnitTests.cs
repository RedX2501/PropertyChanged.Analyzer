// This file is part of PropertyChanged.Fody.Analyzer.
//
// PropertyChanged.Fody.Analyzer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PropertyChanged.Fody.Analyzer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PropertyChanged.Fody.Analyzer.  If not, see <https://www.gnu.org/licenses/>.

namespace PropertyChanged.Fody.Analyzer.Test
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PropertyChanged.Fody.Analyzer.Test.Helpers;

    [TestClass]
    public class AttributeUnitTests : DiagnosticVerifier
    {
        [TestMethod]
        public void Analyze_ClassHasNoProperties_WarningForUnnecessaryAttribute()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    [PropertyChanged.AddINotifyPropertyChangedInterfaceAttribute]
    class NoProperties
    {
    }
}";

            var d = new DiagnosticResult
            {
                Id = PropertyChangedAnalyzer.UnnecessaryAddInterfaceAttributeId,
                Severity = DiagnosticSeverity.Warning,
                Locations = AnyLocation,
                Message = AnyMessage,
            };
            VerifyCSharpDiagnostic(test, d);
        }

        [TestMethod]
        public void Analyze_ClassHasNoPropertiesWithSetter_WarningForUnnecessaryAttribute()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    [PropertyChanged.AddINotifyPropertyChangedInterfaceAttribute]
    class NoProperties
    {
        public bool AProperty { get; }
    }
}";

            var d = new DiagnosticResult
            {
                Id = PropertyChangedAnalyzer.UnnecessaryAddInterfaceAttributeId,
                Severity = DiagnosticSeverity.Warning,
                Locations = AnyLocation,
                Message = AnyMessage,
            };
            VerifyCSharpDiagnostic(test, d);
        }

        [TestMethod]
        public void Analyze_PropertyHasNoSetterAndDoNotNotifyAttribute_WarningForUnnecessaryAttribute()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    class PropertyWithoutSetterButWithAttribute : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [PropertyChanged.DoNotNotifyAttribute]
        public bool AProperty { get; }
    }
}";

            var d = new DiagnosticResult
            {
                Id = PropertyChangedAnalyzer.UnnecessaryDoNotNotifyAttributeId,
                Severity = DiagnosticSeverity.Warning,
                Locations = AnyLocation,
                Message = AnyMessage,
            };
            VerifyCSharpDiagnostic(test, d);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PropertyChangedAnalyzer();
        }
    }
}

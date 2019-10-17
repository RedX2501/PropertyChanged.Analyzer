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
    public class MethodUnitTests : DiagnosticVerifier
    {
        [TestMethod]
        public void Analyze_MethodHasProperty_NoFinding()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    class NoFindings : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool IsUseful { get; set; }

        private void OnIsUsefulChanged()
        {
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_MethodHasPropertyAndClassUsesAttribute_NoFinding()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    [PropertyChanged.AddINotifyPropertyChangedInterfaceAttribute]
    class NoFindings
    {
        public bool IsUseful { get; set; }

        private void OnIsUsefulChanged()
        {
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_PartialClassMethodHasPropertyInDifferentPart_NoFinding()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    partial class PartialClass
    {
        public bool SomeProp { get; set; }
    }

    partial class PartialClass : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnSomePropChanged()
        {

        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_ClassDoesNotInherit_WarningForNoInheritance()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    class DoesNotInherit
    {
        public bool IsUseful { get; set; }

        private void OnIsUsefulChanged()
        {
        }
    }
}";

            var d = new DiagnosticResult
            {
                Id = PropertyChangedAnalyzer.DoesNotInheritDiagnosticId,
                Severity = DiagnosticSeverity.Warning,
                Locations = AnyLocation,
                Message = AnyMessage,
            };
            VerifyCSharpDiagnostic(test, d);
        }

        [TestMethod]
        public void Analyze_MethodHasNoMatchingPropertyBecauseItIsInBaseClass_NoFinding()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    class BaseClassProp : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool IsWorking { get; set; }

        protected virtual void OnIsWorkingChanged()
        {
        }
    }

    class DerivedClass : BaseClassProp
    {
        protected override void OnIsWorkingChanged(){}
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_MethodHasNoMatchingProperty_WarningForNoProperty()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    class MissingProperty : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnMissingPropertyChanged()
        {
        }
    }
}";

            var d = new DiagnosticResult
            {
                Id = PropertyChangedAnalyzer.NoMatchingPropDiagnosticId,
                Severity = DiagnosticSeverity.Warning,
                Locations = AnyLocation,
                Message = AnyMessage,
            };
            VerifyCSharpDiagnostic(test, d);
        }

        [TestMethod]
        public void Analyze_MethodHasMatchingPropertyButNoSetter_WarningForNoSetter()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    class MissingSetter : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool PropertyWithoutSetter { get; }

        private void OnPropertyWithoutSetterChanged()
        {
        }
    }
}";

            var d = new DiagnosticResult
            {
                Id = PropertyChangedAnalyzer.NoSetterDiagnosticId,
                Severity = DiagnosticSeverity.Warning,
                Locations = AnyLocation,
                Message = AnyMessage,
            };
            VerifyCSharpDiagnostic(test, d);
        }

        [TestMethod]
        public void Analyze_PropertySuppressesNotification_WarningForSuppressed()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    class PropertySuppressesNotifications : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [PropertyChanged.DoNotNotifyAttribute]
        public bool IsUseful { get; set; }

        private void OnIsUsefulChanged()
        {
        }
    }
}";

            var d = new DiagnosticResult
            {
                Id = PropertyChangedAnalyzer.SuppressedNotificationId,
                Severity = DiagnosticSeverity.Warning,
                Locations = AnyLocation,
                Message = AnyMessage,
            };
            VerifyCSharpDiagnostic(test, d);
        }

        [TestMethod]
        public void Analyze_MethodIsStatic_WarningForUnsupportedSignature()
        {
            var test = @"namespace SampleForPropertyChangedAnalyzer
{
    class UnsupportedSignatureIsStatic : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool IsUseful { get; set; }

        private static void OnIsUsefulChanged()
        {
        }
    }
}";

            var d = new DiagnosticResult
            {
                Id = PropertyChangedAnalyzer.UnsupportedMethodSignatureDiagnosticId,
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

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

namespace PropertyChanged.Fody.Analyzer
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PropertyChangedAnalyzer : DiagnosticAnalyzer
    {
        public const string Category = "PropertyChanged.Fody.Analyzer";

#pragma warning disable CS1591 // Document public elements
#pragma warning disable SA1203 // Constants should appear before fields

        /* Method is Target IDs 1 - 999 */
        public const string DoesNotInheritDiagnosticId = "PA0001";
        public static readonly LocalizableString DoesNotInheritTitle = "Method will not be called on change because the class does not inherit from INotifyPropertyChanged or does not have it weaved";
        public static readonly LocalizableString DoesNotInheritMessageFormat = "Class {0} does not inherit from INotifyPropertyChanged. This method will not be called by PropertyChanged.Fody.";
        public static readonly DiagnosticDescriptor DoesNotInheritRule = new DiagnosticDescriptor(DoesNotInheritDiagnosticId, DoesNotInheritTitle, DoesNotInheritMessageFormat, Category, DiagnosticSeverity.Warning, true, customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        public const string NoMatchingPropDiagnosticId = "PA0002";
        public static readonly LocalizableString NoMatchingPropTitle = "No property found for this changed method";
        public static readonly LocalizableString NoMatchingPropMessageFormat = "Property {0} not found for this changed handler. This method will not be called by PropertyChanged.Fody.";
        public static readonly DiagnosticDescriptor NoMatchingPropRule = new DiagnosticDescriptor(NoMatchingPropDiagnosticId, NoMatchingPropTitle, NoMatchingPropMessageFormat, Category, DiagnosticSeverity.Warning, true, customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        public const string NoSetterDiagnosticId = "PA0003";
        public static readonly LocalizableString NoSetterTitle = "Method will not be called on change because property has no setter";
        public static readonly LocalizableString NoSetterMessageFormat = "Property {0} has no setter. This method will not be called by PropertyChanged.Fody.";
        public static readonly DiagnosticDescriptor NoSetterRule = new DiagnosticDescriptor(NoSetterDiagnosticId, NoSetterTitle, NoSetterMessageFormat, Category, DiagnosticSeverity.Warning, true, customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        public const string SuppressedNotificationId = "PA0004";
        public static readonly LocalizableString SuppressedNotificationTitle = "Method will not be called on change because Property is suppressing its change notification via attribute";
        public static readonly LocalizableString SuppressedNotificationFormat = "Property '{0}' is suppressing change notifications with an attribute. This method will not be called by PropertyChanged.Fody.";
        public static readonly DiagnosticDescriptor SuppressedNotificationRule = new DiagnosticDescriptor(SuppressedNotificationId, SuppressedNotificationTitle, SuppressedNotificationFormat, Category, DiagnosticSeverity.Warning, true, customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        public const string UnsupportedMethodSignatureDiagnosticId = "PA0005";
        public static readonly LocalizableString UnsupportedMethodSignatureTitle = "Method will not be called on change because it has an unsupported signature";
        public static readonly LocalizableString UnsupportedMethodSignatureFormat = "Method {0} has an unsupported signature. This method will not be called by PropertyChanged.Fody.";
        public static readonly DiagnosticDescriptor UnsupportedMethodSignatureRule = new DiagnosticDescriptor(UnsupportedMethodSignatureDiagnosticId, UnsupportedMethodSignatureTitle, UnsupportedMethodSignatureFormat, Category, DiagnosticSeverity.Warning, true, customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        /* Attribute Is Target IDs 1000 - 1999 */
        public const string UnnecessaryDoNotNotifyAttributeId = "PA1000";
        public static readonly LocalizableString UnnecessaryDoNotNotifyAttributeTitle = "Attribute is unncessary because property has no setter";
        public static readonly LocalizableString UnnecessaryDoNotNotifyAttributeFormat = "'DoNotNotifyAttribute' is unnecessary because property {0} has no setter.";
        public static readonly DiagnosticDescriptor UnnecessaryDoNotNotifyAttributeRule = new DiagnosticDescriptor(UnnecessaryDoNotNotifyAttributeId, UnnecessaryDoNotNotifyAttributeTitle, UnnecessaryDoNotNotifyAttributeFormat, Category, DiagnosticSeverity.Warning, true, customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        public const string UnnecessaryAddInterfaceAttributeId = "PA1001";
        public static readonly LocalizableString UnnecessaryAddInterfaceAttributeTitle = "Attribute is unncessary because class has no property";
        public static readonly LocalizableString UnnecessaryAddInterfaceAttributeFormat = "'AddINotifyPropertyChangedInterfaceAttribute' is unnecessary because class {0} has no properties or no properties with setters.";
        public static readonly DiagnosticDescriptor UnnecessaryAddInterfaceAttributeRule = new DiagnosticDescriptor(UnnecessaryAddInterfaceAttributeId, UnnecessaryAddInterfaceAttributeTitle, UnnecessaryAddInterfaceAttributeFormat, Category, DiagnosticSeverity.Warning, true, customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        /* Class Is Target IDs 2000 - 2999 */

#pragma warning restore SA1203 // Constants should appear before fields
#pragma warning restore CS1591 // Document public elements

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DoesNotInheritRule,
            NoMatchingPropRule,
            NoSetterRule,
            SuppressedNotificationRule,
            UnsupportedMethodSignatureRule,
            UnnecessaryDoNotNotifyAttributeRule,
            UnnecessaryAddInterfaceAttributeRule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext obj)
        {
            var classDecl = (ClassDeclarationSyntax)obj.Node;
            var classSymbol = obj.SemanticModel.GetDeclaredSymbol(classDecl);
            var classMembers = classSymbol.GetMembers();

            // maybe we can find a way of caching this?
            var propChangedInterfaceType = obj.Compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName);

            DiagnoseClassHasUnnecessaryAttribute(classSymbol, obj.ReportDiagnostic, classDecl);

            var inherits = InheritsFromINotifyPropertyChanged(classSymbol, propChangedInterfaceType);

            foreach (var classMember in classMembers)
            {
                switch (classMember)
                {
                    case IPropertySymbol propertySymbol:
                        DiagnosePropertyHasUnnecessaryAttribute(propertySymbol, obj.ReportDiagnostic);

                        break;

                    case IMethodSymbol methodSymbol:
                        // must contain at least 'OnChanged' and an additional character
                        if (methodSymbol.Name.Length <= 9 || !(methodSymbol.Name.StartsWith("On") && methodSymbol.Name.EndsWith("Changed")))
                        {
                            continue;
                        }

                        // If the method has the correct name but is overriding something in the base class
                        // it is very likely that the property is also in the base class. Therefore we ignore it.
                        if (!(methodSymbol.OverriddenMethod is null))
                        {
                            continue;
                        }

                        if (!inherits)
                        {
                            var doesNotInheritDiag = Diagnostic.Create(DoesNotInheritRule, methodSymbol.Locations.First(), classSymbol.Name);
                            obj.ReportDiagnostic(doesNotInheritDiag);
                        }

                        DiagnoseWhenMethodHasUnsupportedSignature(methodSymbol, obj.ReportDiagnostic);

                        var expectedPropName = methodSymbol.Name.Substring(2, methodSymbol.Name.Length - 7 - 2);

                        // While the user is editing symbols for a short time multiple properties with the same name can exist. SingleOrDefault does not work
                        // in that case.
                        // Assuming that any one of those we get is as wrong as the next one, we fetch the first one and see if that works.
                        // Once the user is done editing only one should be left and we'll check the correct one.
                        var namedSymbol = classMembers.FirstOrDefault(x => x.Name.Equals(expectedPropName, StringComparison.Ordinal));
                        if (namedSymbol is IPropertySymbol propSymbol)
                        {
                            var hasSetter = propSymbol.SetMethod != null;
                            if (!hasSetter)
                            {
                                var missingSetterDiag = Diagnostic.Create(NoSetterRule, methodSymbol.Locations.First(), expectedPropName);
                                obj.ReportDiagnostic(missingSetterDiag);
                            }

                            if (propSymbol.GetAttributes().Any(x => x.AttributeClass.Name == "DoNotNotifyAttribute"))
                            {
                                var suppressedChangeNotificationDiag = Diagnostic.Create(SuppressedNotificationRule, methodSymbol.Locations.First(), propSymbol.Name);
                                obj.ReportDiagnostic(suppressedChangeNotificationDiag);
                            }
                        }
                        else
                        {
                            var noMatchingPropDiag = Diagnostic.Create(NoMatchingPropRule, methodSymbol.Locations.First(), expectedPropName);
                            obj.ReportDiagnostic(noMatchingPropDiag);
                        }

                        break;
                }
            }
        }

        private bool InheritsFromINotifyPropertyChanged(INamedTypeSymbol classSymbol, INamedTypeSymbol propChangedInterfaceType)
        {
            if (classSymbol.AllInterfaces.Any(x => ReferenceEquals(x, propChangedInterfaceType)) ||
                classSymbol.GetAttributes().Any(x => x.AttributeClass.Name.Equals("AddINotifyPropertyChangedInterfaceAttribute")))
            {
                return true;
            }

            // maybe some class has the attribute?
            var baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                if (InheritsFromINotifyPropertyChanged(baseType, propChangedInterfaceType))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        private void DiagnosePropertyHasUnnecessaryAttribute(IPropertySymbol propertySymbol, Action<Diagnostic> reportDiagnostic)
        {
            var attr = propertySymbol.GetAttributes().SingleOrDefault(x => x.AttributeClass.Name.Equals("DoNotNotifyAttribute"));
            if (attr == null)
            {
                return;
            }

            if (propertySymbol.SetMethod == null)
            {
                var attrSyntax = attr.ApplicationSyntaxReference.GetSyntax();
                var unnecessaryAttributeDiag = Diagnostic.Create(UnnecessaryDoNotNotifyAttributeRule, attrSyntax.GetLocation(), propertySymbol.Name);
                reportDiagnostic(unnecessaryAttributeDiag);
            }
        }

        private void DiagnoseClassHasUnnecessaryAttribute(INamedTypeSymbol classSymbol, Action<Diagnostic> reportDiagnostic, ClassDeclarationSyntax classDecl)
        {
            // During copy paste operations the same class will have the attribute several times
            var attr = classSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name.Equals("AddINotifyPropertyChangedInterfaceAttribute"));
            if (attr == null)
            {
                return;
            }

            var properties = classSymbol.GetMembers().OfType<IPropertySymbol>();
            if (properties.All(x => x.SetMethod == null))
            {
                var attrSyntax = attr.ApplicationSyntaxReference.GetSyntax();
                var unnecessaryAttributeDiag = Diagnostic.Create(UnnecessaryAddInterfaceAttributeRule, attrSyntax.GetLocation(), classSymbol.Name);
                reportDiagnostic(unnecessaryAttributeDiag);
            }
        }

        private void DiagnoseWhenMethodHasUnsupportedSignature(IMethodSymbol methodSymbol, Action<Diagnostic> reportDiagnostic)
        {
            if (methodSymbol.IsStatic)
            {
                var unsupportedSignatureDiag = Diagnostic.Create(UnsupportedMethodSignatureRule, methodSymbol.Locations.First(), methodSymbol.Name);
                reportDiagnostic(unsupportedSignatureDiag);
            }
        }
    }
}

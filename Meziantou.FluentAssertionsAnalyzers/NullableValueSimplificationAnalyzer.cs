using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullableValueSimplificationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor SimplifyHasValueBeTrueRule = new(
        "MFA014",
        title: "Simplify .HasValue.Should().BeTrue()",
        messageFormat: "Simplify .HasValue.Should().BeTrue()",
        description: "",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor SimplifyHasValueBeFalseRule = new(
        "MFA015",
        title: "Simplify .HasValue.Should().BeFalse()",
        messageFormat: "Simplify .HasValue.Should().BeFalse()",
        description: "",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(SimplifyHasValueBeTrueRule, SimplifyHasValueBeFalseRule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(ctx =>
        {
            ctx.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        });
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var op = (IInvocationOperation) context.Operation;

        var rule = op.TargetMethod.Name switch
        {
            "BeTrue" => SimplifyHasValueBeTrueRule,
            "BeFalse" => SimplifyHasValueBeFalseRule,
            _ => null
        };

        if (rule == null)
            return;

        var shouldOperation = op.Children.OfType<IInvocationOperation>().FirstOrDefault(x => x.TargetMethod.Name.Equals("Should", StringComparison.Ordinal));

        var hasValueOperation = (IMemberReferenceOperation) shouldOperation?.Children.OfType<IArgumentOperation>()
            .FirstOrDefault(x => x.Value is IMemberReferenceOperation hasValue && hasValue.Member.Name.Equals("HasValue", StringComparison.Ordinal))
            ?.Value;

        if (hasValueOperation != null && hasValueOperation.Member.ContainingType.ConstructedFrom.SpecialType.Equals(SpecialType.System_Nullable_T))
        {
            context.ReportDiagnostic(Diagnostic.Create(rule, op.Syntax.GetLocation()));
        }
    }
}
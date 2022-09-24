using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Meziantou.FluentAssertionsAnalyzers.CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class NullableValueSimplificationCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MFA014", "MFA015");

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);

        if (nodeToFix == null)
        {
            return;
        }

        const string title = "Simplify to *.Should().HaveValue()";

        var codeAction = CodeAction.Create(
            title,
            ct => Rewrite(context.Document, nodeToFix, ct),
            equivalenceKey: title);

        context.RegisterCodeFix(codeAction, context.Diagnostics);
    }

    private async Task<Document> Rewrite(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var originalMethod = (InvocationExpressionSyntax) nodeToFix;

        var rootExpression = originalMethod.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
            .FirstOrDefault(x => x.Name.Identifier.Text.Equals("HasValue", StringComparison.Ordinal))?.Expression;

        editor.ReplaceNode(originalMethod, InvocationExpression(
            MemberAccessExpression(InvokeShould(rootExpression), "HaveValue")));

        return editor.GetChangedDocument();
    }

    private static InvocationExpressionSyntax InvokeShould(ExpressionSyntax expression)
    {
        return InvocationExpression(
            MemberAccessExpression(Parenthesize(expression), "Should"));
    }

    private static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, string memberName)
    {
        return SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            expression,
            IdentifierName(memberName));
    }

    private static ExpressionSyntax Parenthesize(ExpressionSyntax expression)
    {
        var withoutTrivia = expression.WithoutTrivia();
        var parenthesized = ParenthesizedExpression(withoutTrivia);

        return parenthesized.WithTriviaFrom(expression).WithAdditionalAnnotations(Simplifier.Annotation);
    }
}
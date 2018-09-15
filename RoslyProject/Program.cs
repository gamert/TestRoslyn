using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Walterlv.Demo.Roslyn
{

    class Program
    {
        static void Main(string[] args)
        {
            RoslynRename r = new RoslynRename();
            r.RunAsync().Wait();
        }

    }

    public class RoslynRename
    {
        MSBuildWorkspace m_workspace;
        public async Task RunAsync()
        {
            m_workspace = MSBuildWorkspace.Create();
            Solution solution = await m_workspace.OpenSolutionAsync(@"G:\GitHub\dotnet\GitHubRoslynTest\Samples\ConsoleApp1\ConsoleApp1.sln");
            Project project = solution.Projects.First(x => x.Name == "ConsoleApp1");
            Document document = project.Documents.First(x =>
                x.Name.Equals("ACTAnimConfig.cs", StringComparison.InvariantCultureIgnoreCase));

            var tree = await document.GetSyntaxTreeAsync();
            MethodDeclarationSyntax TestAnim = FindFuncAsync(tree, "ACTAnimConfig", "TestAnim");
            if(TestAnim!=null)
            {
                var newName = "_ZTest" + TestAnim.Identifier;
                await RenameAsync(document, TestAnim, newName);
            }
            //var text = node.GetText();
            //File.WriteAllText(document.FilePath, text.ToString());
        }

        async Task RenameAsync(Document document, SyntaxNode typeDecl,string newName)
        {
            CancellationToken cancellationToken = default(CancellationToken);
            // Produce a reversed version of the type declaration's identifier token.

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);
            bool b = m_workspace.TryApplyChanges(newSolution);
            //check...
            /*
             * Assets/Editor/ACTAnimation/ACTAnimConfigInspector.cs(33,25): 
             * error CS1061: Type `ACTAnimConfig' does not contain a definition for `TestAnim' 
             * and no extension method `TestAnim' of type `ACTAnimConfig' could be found. 
             * Are you missing an assembly reference?
             */
        }

        //"ACTAnimConfig"
        MethodDeclarationSyntax FindFuncAsync(SyntaxTree tree, string type,string func)
        {
            var syntax = tree.GetCompilationUnitRoot();

            TypeParameterVisitor visitor = new TypeParameterVisitor(type);
            SyntaxNode node = visitor.Visit(syntax);

            if (visitor.typeDecl1 != null)
            {
                foreach (MemberDeclarationSyntax member in visitor.typeDecl1.Members)
                {
                    if (member is MethodDeclarationSyntax)
                    {
                        MethodDeclarationSyntax ed = member as MethodDeclarationSyntax;
                        if (ed.Identifier.Text == func)
                        {
                            return ed;
                        }
                    }
                }
            }
            return null;
        }

        //find the named ClassDeclarationSyntax
        class TypeParameterVisitor : CSharpSyntaxRewriter
        {
            string m_key;
            public ClassDeclarationSyntax typeDecl1 = null;
            public TypeParameterVisitor(string key)
            {
                m_key = key;
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if(node.Identifier.ValueText == m_key) 
                    typeDecl1 = node;

                return node;// node.Update(lessThanToken, syntaxList, greaterThanToken);
            }
        }
    }



}
 
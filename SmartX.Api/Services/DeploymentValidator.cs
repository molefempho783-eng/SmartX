using SmartX.Api.Models;


namespace SmartX.Api.Services;

/// Recursively validates the deployment tree (IIE, 2026).

public class DeploymentValidator
{
    private readonly List<string> _errors = new();

    public List<string> Validate(DeploymentNode node)
    {
        _errors.Clear();
        ValidateRecursive(node, "");
        return _errors;
    }

    // Recursive method with path tracking (IIE, 2026).
    private void ValidateRecursive(DeploymentNode node, string currentPath)
    {
        // Build hierarchical path for error reporting (IIE, 2026).
        string fullPath = string.IsNullOrEmpty(currentPath)
            ? node.Name
            : $"{currentPath} > {node.Name}";

        // Rule 1: Every node must have a name (IIE, 2026).
        if (string.IsNullOrWhiteSpace(node.Name))
        {
            _errors.Add($"Node at {fullPath} has no name.");
        }

        // Rule 2: Base case, leaf node (Sensor) must be configured
        // (Sedgewick and Wayne, 2011).
        if (node.Children.Count == 0 && !node.IsConfigured)
        {
            _errors.Add($"Sensor '{fullPath}' is not configured.");
        }

        // Rule 3: Container nodes must have at least one child if marked configured
        // (IIE, 2026).
        if (node.Children.Count > 0 && !node.IsConfigured)
        {
            _errors.Add($"Container '{fullPath}' is marked unconfigured but has {node.Children.Count} children.");
        }

        // RECURSIVE CASE: validate each child (Sedgewick and Wayne, 2011).
        foreach (var child in node.Children)
        {
            ValidateRecursive(child, fullPath);
        }
    }
}

/* Reference List
IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Sedgewick, R. and Wayne, K., 2011. Algorithms. 4th ed. Boston: Addison-Wesley.
*/
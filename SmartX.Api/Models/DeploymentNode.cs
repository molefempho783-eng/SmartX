using System.Collections.Generic;

//Represents a node in the physical deployment hierarchy:

namespace SmartX.Api.Models;
public class DeploymentNode
{
    public string Name { get; set; } = string.Empty;

    // Node type: Facility, Zone, Sub-Zone, Sensor (IIE, 2026).
    public string NodeType { get; set; } = string.Empty;

    // Configuration state to be validated recursively (IIE, 2026).
    public bool IsConfigured { get; set; }

    // Self-referencing list for recursive tree traversal (Sedgewick and Wayne, 2011).
    public List<DeploymentNode> Children { get; set; } = new();
}

/* Reference List
IIE, 2026. PROG7312 Module Manual. The Independent Institute of Education (Pty) Ltd.

Sedgewick, R. and Wayne, K., 2011. Algorithms. 4th ed. Boston: Addison-Wesley.
*/
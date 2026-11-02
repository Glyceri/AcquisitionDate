using System.Numerics;
using AcquisitionDate.Translation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace AcquisitionDate.Nodes;

internal class AchievementDateResNode : ResNode
{
    public const uint NODE_ID           = 100;
    public const uint DATE_TEXT_NODE_ID = NODE_ID + 3;
    public const uint ALT_NODE_ID       = NODE_ID + 4;
    
    public readonly TextNode LabelNode;
    public readonly TextNode ColonNode;
    public readonly TextNode DateTextNode;
    public readonly TextNode AltNameTextNode;
    
    public AchievementDateResNode()
    {
        NodeId   = NODE_ID;
        Position = new Vector2(48, 4);
        Size     = new Vector2(360, 18);
        
        LabelNode = new TextNode
        {
            NodeId   = NODE_ID + 1,
            Position = Vector2.Zero,
            Size     = new Vector2(80, 18),
            String   = Translator.GetLine("AchievedOn"),
            AlignmentType = AlignmentType.Right,
        };
        LabelNode.AttachNode(this);
        
        ColonNode = new TextNode
        {
            NodeId   = NODE_ID + 2,
            Position = new Vector2(80, 0),
            Size     = new Vector2(10, 18),
            String   = ": ",
            AlignmentType = AlignmentType.Center,
        };
        ColonNode.AttachNode(this);
        
        DateTextNode = new TextNode
        {
            NodeId   = DATE_TEXT_NODE_ID,
            Position = new Vector2(90, 0),
            Size     = new Vector2(80, 18),
            String   = "--/--/----",
            AlignmentType = AlignmentType.Right,
        };
        DateTextNode.AttachNode(this);
        
        AltNameTextNode = new TextNode
        {
            NodeId   = ALT_NODE_ID,
            Position = new Vector2(180, 0),
            Size     = new Vector2(160, 18),
            String   = "(TEMP ALT NAME@HOMEWORLD)",
            FontSize = 9,
            AlignmentType = AlignmentType.Left,
        };
        AltNameTextNode.AttachNode(this);
    }
}
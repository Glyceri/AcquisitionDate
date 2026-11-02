using System.Numerics;
using AcquisitionDate.Translation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace AcquisitionDate.Nodes;

internal class FishGuideResNode : ResNode
{
    public readonly TextNode LabelNode;
    public readonly TextNode DateTextNode;
    
    public FishGuideResNode()
    {
        Size      = new Vector2(280, 14);
        Position  = new Vector2(714, 52);
        
        LabelNode = new TextNode
        {
            Size             = new Vector2(120, 18),
            AlignmentType    = AlignmentType.Left,
            String           = Translator.GetLine("AcquiredOn") + ": ",
            TextColor        = new Vector4(0.671f, 0.471f, 0.322f, 1.0f),
            TextFlags        = TextFlags.None,
            FontSize         = 12,
        };
        LabelNode.AttachNode(this);
        
        DateTextNode = new TextNode
        {
            Position         = new Vector2(120, 0),
            Size             = new Vector2(80, 18),
            AlignmentType    = AlignmentType.Right,
            String           = string.Empty,
            TextColor        = new Vector4(0.671f, 0.471f, 0.322f, 1.0f),
            TextFlags        = TextFlags.None,
            FontSize         = 12,
        };
        DateTextNode.AttachNode(this);
    }
}
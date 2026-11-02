using System.Numerics;
using AcquisitionDate.Translation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace AcquisitionDate.Nodes;

internal class EorzeaIncognitaResNode : ResNode
{
    public readonly TextNode LabelNode;
    public readonly TextNode DateTextNode;
    
    public EorzeaIncognitaResNode()
    {
        Size      = new Vector2(280, 14);
        Position  = new Vector2(484, 254);
        
        LabelNode = new TextNode
        {
            Size             = new Vector2(120, 18),
            AlignmentType    = AlignmentType.Left,
            String           = Translator.GetLine("AcquiredOn") + ": ",
            TextColor        = new Vector4(0.631f, 0.467f, 0.631f, 1.0f),
            TextFlags        = TextFlags.None,
            FontSize         = 14,
        };
        LabelNode.AttachNode(this);
        
        DateTextNode = new TextNode
        {
            Position         = new Vector2(120, 0),
            Size             = new Vector2(80, 18),
            AlignmentType    = AlignmentType.Right,
            String           = string.Empty,
            TextColor        = new Vector4(0.631f, 0.467f, 0.631f, 1.0f),
            TextFlags        = TextFlags.None,
            FontSize         = 14,
        };
        DateTextNode.AttachNode(this);
    }
}
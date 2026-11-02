using System.Numerics;
using AcquisitionDate.Translation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace AcquisitionDate.Nodes;

internal class GlassSelectDateResNode : ResNode
{
    public readonly TextNode LabelNode;
    public readonly TextNode DateTextNode;
    
    public GlassSelectDateResNode()
    {
        Size      = new Vector2(280, 14);
        Position  = new Vector2(20, 498);
        
        LabelNode = new TextNode
        {
            Size             = new Vector2(120, 18),
            AlignmentType    = AlignmentType.Left,
            String           = Translator.GetLine("AcquiredOn") + ": ",
            TextColor        = new Vector4(0.612f, 0.380f, 0.086f, 1.0f),
            TextOutlineColor = new Vector4(1.000f, 0.694f, 0.490f, 1.0f),
            TextFlags        = TextFlags.None | TextFlags.Edge,
            FontSize         = 12,
            FontType         = FontType.MiedingerMed,
        };
        LabelNode.AttachNode(this);
        
        DateTextNode = new TextNode
        {
            Position         = new Vector2(0, 18),
            Size             = new Vector2(80, 18),
            AlignmentType    = AlignmentType.Left,
            String           = string.Empty,
            TextColor        = new Vector4(0.612f, 0.380f, 0.086f, 1.0f),
            TextOutlineColor = new Vector4(1.000f, 0.694f, 0.490f, 1.0f),
            TextFlags        = TextFlags.None | TextFlags.Edge,
            FontSize         = 12,
            FontType         = FontType.MiedingerMed,
        };
        DateTextNode.AttachNode(this);
    }
}
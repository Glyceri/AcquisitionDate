using System.Numerics;
using AcquisitionDate.Translation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace AcquisitionDate.Nodes;

internal class CharacterClassResNode : ResNode
{
    public readonly TextNode LabelNode;
    public readonly TextNode ColonNode;
    public readonly TextNode DateTextNode;
    
    public CharacterClassResNode()
    {
        Size      = new Vector2(315, 18);
        Position  = new Vector2(14, 530);
        IsVisible = false;
        
        LabelNode = new TextNode
        {
            Size             = new Vector2(165, 18),
            AlignmentType    = AlignmentType.Right,
            String           = Translator.GetLine("AchievedOn"),
            TextColor        = new Vector4(0.612f, 0.380f, 0.086f, 1.0f),
            TextOutlineColor = new Vector4(1.0f, 0.694f, 0.490f, 1.0f),
            TextFlags        = TextFlags.Glare | TextFlags.Edge | TextFlags.Emboss,
        };
        LabelNode.AttachNode(this);
        
        ColonNode  = new TextNode
        {
            Position         = new Vector2(165, 0),
            Size             = new Vector2(12, 18),
            AlignmentType    = AlignmentType.Center,
            String           = ": ",
            TextColor        = new Vector4(0.612f, 0.380f, 0.086f, 1.0f),
            TextOutlineColor = new Vector4(1.0f, 0.694f, 0.490f, 1.0f),
            TextFlags        = TextFlags.Glare | TextFlags.Edge | TextFlags.Emboss,
        };
        ColonNode.AttachNode(this);
        
        DateTextNode = new TextNode
        {
            Position         = new Vector2(177, 0),
            Size             = new Vector2(200, 18),
            AlignmentType    = AlignmentType.Left,
            String           = string.Empty,
            TextColor        = new Vector4(0.612f, 0.380f, 0.086f, 1.0f),
            TextOutlineColor = new Vector4(1.0f, 0.694f, 0.490f, 1.0f),
            TextFlags        = TextFlags.Glare | TextFlags.Edge | TextFlags.Emboss,
        };
        DateTextNode.AttachNode(this);
    }
}
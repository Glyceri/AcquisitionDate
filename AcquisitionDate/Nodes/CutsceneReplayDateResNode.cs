using System.Numerics;
using AcquisitionDate.Translation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace AcquisitionDate.Nodes;

internal class CutsceneReplayDateResNode : ResNode
{
    public readonly TextNode LabelNode;
    public readonly TextNode DateTextNode;
    
    public CutsceneReplayDateResNode()
    {
        Size      = new Vector2(280, 14);
        Position  = new Vector2(250, 100);
        
        LabelNode = new TextNode
        {
            Size             = new Vector2(120, 18),
            AlignmentType    = AlignmentType.Left,
            String           = Translator.GetLine("CompletedOn") + ": ",
            TextColor        = new Vector4(0.2f, 0.2f, 0.2f, 1.0f),
            TextFlags        = TextFlags.None,
            FontSize         = 12,
            FontType         = FontType.Axis,
        };
        LabelNode.AttachNode(this);
        
        DateTextNode = new TextNode
        {
            Position         = new Vector2(120, 0),
            Size             = new Vector2(80, 18),
            AlignmentType    = AlignmentType.Right,
            String           = string.Empty,
            TextColor        = new Vector4(0.2f, 0.2f, 0.2f, 1.0f),
            TextFlags        = TextFlags.None,
            FontSize         = 12,
            FontType         = FontType.Axis,
        };
        DateTextNode.AttachNode(this);
    }
}
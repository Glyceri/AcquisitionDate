using System.Linq;
using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Nodes;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using CAchievement = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class AchievementWindowHook : DateTextHook
{
    private static readonly uint[] ListRenderers = [4, 41001, 41002, 41003, 41004, 41005, 41006, 41007, 41008];
    
    public AchievementWindowHook(IDatabase database, IAcquisitionServices services)
        : base(database, services)
        { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "Achievement", OnAchievementUpdate);
    }

    protected override void OnDispose()
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(OnAchievementUpdate);
    }
    
    protected override bool HandleConfig(Configuration configuration)
        => configuration.DrawDatesOnAchievements;
    
    private void OnAchievementUpdate(AddonEvent type, AddonArgs args)
    {
        AtkUnitBase* unitBase = (AtkUnitBase*)args.Addon.Address;
        
        if (unitBase == null)
        {
            return;
        }
        
        AtkComponentList* cList = unitBase->GetComponentListById(39);
        
        if (cList == null)
        {
            return;
        }
        
        for (int i = 0; i < cList->ColumnCount; i++)
        {
            AtkComponentListItemRenderer* cBase = cList->GetItemRenderer(i);
            
            if (cBase == null)
            {
                continue;
            }
            
            if (!ListRenderers.Contains(cBase->ComponentNode->NodeId))
            {
                continue;
            }
            
            AtkResNode* holderNode = cBase->GetNodeById(5);
            
            if (holderNode == null)
            {
                continue;
            }
            
            AtkTextNode* textComponent = cBase->GetTextNodeById(8);
            
            if (textComponent == null)
            {
                continue;
            }
            
            if (cBase->GetNodeById(AchievementDateResNode.NODE_ID) == null)
            {
                new AchievementDateResNode()
                    .AttachNode(holderNode);
            }
            
            AtkResNode* myResNode = cBase->GetNodeById(AchievementDateResNode.NODE_ID);
            
            if (myResNode == null)
            {
                continue;
            }
            
            bool draw = DrawDateNode(textComponent, cBase, myResNode);
            
            textComponent->IsDirty = true;
            
            if (!draw)
            {
                textComponent->Height = 46;
                textComponent->Y      = 0;
            }
            else
            {
                textComponent->Height = 23;
                textComponent->Y      = 20;
            }
        }
    }
    
    private bool DrawDateNode(AtkTextNode* achieveNode, AtkComponentListItemRenderer* baseNode, AtkResNode* myResNode)
    {
        myResNode->ToggleVisibility(false);
        
        AtkTextNode* dateTextNode = baseNode->GetTextNodeById(AchievementDateResNode.DATE_TEXT_NODE_ID);
        AtkTextNode* altTextNode  = baseNode->GetTextNodeById(AchievementDateResNode.ALT_NODE_ID);
        
        if (dateTextNode == null || altTextNode == null)
        {
            return false;
        }
        
        string       achievementName = achieveNode->NodeText.ToString().Trim();
        Achievement? achievement     = Services.Sheets.GetAchievement(achievementName);
        
        if (achievement == null)
        {
            return false;
        }
        
        uint rowId = achievement.Value.RowId;
        bool draw  = DrawDateString(dateTextNode, altTextNode, DrawTextFlags.AllowAlts, AcquirableDateType.Achievement, rowId);
        
        if (altTextNode->NodeText.ExtractText().IsNullOrWhitespace() && 
            !CAchievement.Instance()->IsComplete((int)rowId) && 
            !Services.Configuration.ShowPlaceholderDates)
        {
            return false;
        }
        
        myResNode->ToggleVisibility(draw);
        
        return draw;
    }

    // Pending removal VVVV
    
    protected override IDatableList GetList(IDatableData userData) 
        => userData.GetDateList(AcquirableDateType.Achievement);
    
    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
        { }
}

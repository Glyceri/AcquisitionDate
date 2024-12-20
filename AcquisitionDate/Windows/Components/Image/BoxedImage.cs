﻿using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using AcquistionDate.PetNicknames.Windowing.Components.Image.UldHelpers;
using AcquistionDate.PetNicknames.Windowing.Components.Texture;
using System.Numerics;
using AcquiryDate.PetNicknames.Services.ServiceWrappers.Interfaces;
using AcquisitionDate;
using AcquisitionDate.Core.Handlers;

namespace AcquistionDate.PetNicknames.Windowing.Components.Image;

internal static class BoxedImage
{
    public static void Draw(IDalamudTextureWrap textureWrap, Vector2 size)
    {
        if (Listbox.Begin("##image", size))
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(style.FramePadding.X, style.FramePadding.Y));
            IconImage.Draw(textureWrap, ImGui.GetContentRegionAvail() - style.FramePadding);
            Listbox.End();
        }
    }

    public static void DrawMinion(IPetSheetData data, Configuration configuration, Vector2 size)
    {
        IDalamudTextureWrap textureWrap;

        UldIcon? raceIcon = null;

        ImGuiStylePtr stylePtr = ImGui.GetStyle();
        float framePaddingX = stylePtr.FramePadding.X;
        float framePaddingY = stylePtr.FramePadding.Y;

        if (data.Model <= -1) 
        {
            textureWrap = PluginHandlers.TextureProvider.GetFromGameIcon(data.Icon).GetWrapOrEmpty(); 
        }
        else
        {
            uint adder = 0;

            if (configuration.minionIconType == 1)
            {
                adder = 64000;
            }
            else if (configuration.minionIconType == 2)
            {
                adder = 55000;
            }

            textureWrap = PluginHandlers.TextureProvider.GetFromGameIcon(data.Icon + adder).GetWrapOrEmpty();
            raceIcon = RaceIconHelper.GetFromRaceID(data?.RaceID ?? 0);
        }

        
        IconImage.Draw(textureWrap, size);
        ImGui.SameLine(0, 0);
        
        if (raceIcon != null)
        {
            Vector2 iconSize = new Vector2(size.X * 0.193f, size.Y * 0.191f);
            Vector2 calculation = new Vector2(iconSize.X * 1.54f, -iconSize.Y * 0.1f);
            Vector2 cursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(ImGui.GetCursorPos() - calculation);
            IconImage.DrawUld(raceIcon.Value, iconSize);
            ImGui.SetCursorPos(cursorPos);
        }
    }
}

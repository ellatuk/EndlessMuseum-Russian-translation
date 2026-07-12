using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using xTile;

namespace EndlessMuseum;

internal sealed class MapNineSlice
{
    public const string MAP_SECTION = $"Maps/{ModEntry.ModId}_section";
    public const int MIN_ROW_TO_BREAK = 6;

    private readonly Rectangle SECTION_TL = new(0, 0, 3, 5);
    private readonly Rectangle SECTION_TC = new(3, 0, 2, 5);
    private readonly Rectangle SECTION_TR = new(5, 0, 3, 5);
    private readonly Rectangle SECTION_ML = new(0, 5, 3, 5);
    private readonly Rectangle SECTION_MC = new(3, 5, 2, 5);
    private readonly Rectangle SECTION_MR = new(5, 5, 3, 5);
    private readonly Rectangle SECTION_BL = new(0, 10, 3, 2);
    private readonly Rectangle SECTION_BC = new(3, 10, 2, 2);
    private readonly Rectangle SECTION_BR = new(5, 10, 3, 2);

    private readonly Rectangle SECTION_BREAK_T = new(8, 0, 4, 5);
    private readonly Rectangle SECTION_BREAK_M = new(8, 5, 4, 5);
    private readonly Rectangle SECTION_BREAK_B = new(8, 10, 4, 2);

    public int GetMaxColInRow(int displayWidth, int baseX)
    {
        int maxColInRow = (int)
            MathF.Floor(
                ((float)(displayWidth / Game1.tileSize - baseX) - SECTION_TL.Width - SECTION_TR.Width)
                    / SECTION_TC.Width
            );

        if (maxColInRow < MIN_ROW_TO_BREAK)
            return maxColInRow;
        maxColInRow -= 2;
        return maxColInRow;
    }

    public void Patch(IAssetDataForMap data, Point origin, int rows, int cols)
    {
        Map source = Game1.game1.xTileContent.Load<Map>(MAP_SECTION);
        data.ExtendMap(0, origin.Y + SECTION_TL.Height - 1 + rows * (SECTION_ML.Height - 1) + SECTION_BL.Height);
        // left col
        PatchSection(data, source, SECTION_TL, origin, 0, 0);
        for (int j = 0; j < rows; j++)
        {
            PatchSection(data, source, SECTION_ML, origin, 0, SECTION_TL.Height - 1 + j * (SECTION_ML.Height - 1));
        }
        PatchSection(data, source, SECTION_BL, origin, 0, SECTION_TL.Height - 1 + rows * (SECTION_ML.Height - 1));
        // mid cols
        int xBreak = 0;
        if (cols < MIN_ROW_TO_BREAK)
        {
            // one table case
            PatchCols(data, source, origin, rows, 0, cols, 0);
        }
        else
        {
            // three table case
            int oneThird = cols / 3;
            int remainder = cols % 3;
            xBreak = (SECTION_BREAK_T.Width - SECTION_MC.Width) * 2;
            switch (remainder)
            {
                case 0:
                    PatchCols(data, source, origin, rows, 0, oneThird - 1, 0);
                    PatchGap(data, source, origin, rows, oneThird - 1, 0);
                    PatchCols(data, source, origin, rows, oneThird - 1, oneThird, SECTION_BREAK_T.Width);
                    PatchGap(data, source, origin, rows, oneThird * 2 - 1, SECTION_BREAK_T.Width);
                    PatchCols(data, source, origin, rows, oneThird * 2 - 1, oneThird - 1, SECTION_BREAK_T.Width * 2);
                    break;
                case 1:
                    PatchCols(data, source, origin, rows, 0, oneThird, 0);
                    PatchGap(data, source, origin, rows, oneThird, 0);
                    PatchCols(data, source, origin, rows, oneThird, oneThird - 1, SECTION_BREAK_T.Width);
                    PatchGap(data, source, origin, rows, oneThird * 2 - 1, SECTION_BREAK_T.Width);
                    PatchCols(data, source, origin, rows, oneThird * 2 - 1, oneThird, SECTION_BREAK_T.Width * 2);
                    break;
                case 2:
                    PatchCols(data, source, origin, rows, 0, oneThird, 0);
                    PatchGap(data, source, origin, rows, oneThird, 0);
                    PatchCols(data, source, origin, rows, oneThird, oneThird, SECTION_BREAK_T.Width);
                    PatchGap(data, source, origin, rows, oneThird * 2, SECTION_BREAK_T.Width);
                    PatchCols(data, source, origin, rows, oneThird * 2, oneThird, SECTION_BREAK_T.Width * 2);
                    break;
            }
        }

        // right col
        int x2 = SECTION_TL.Width + cols * SECTION_MC.Width + xBreak;
        PatchSection(data, source, SECTION_TR, origin, x2, 0);
        for (int j = 0; j < rows; j++)
        {
            PatchSection(data, source, SECTION_MR, origin, x2, SECTION_TR.Height - 1 + j * (SECTION_MR.Height - 1));
        }
        PatchSection(data, source, SECTION_BR, origin, x2, SECTION_TR.Height - 1 + rows * (SECTION_MR.Height - 1));
    }

    private void PatchCols(IAssetDataForMap data, Map source, Point origin, int rows, int start, int count, int xOffset)
    {
        for (int i = start; i < start + count; i++)
        {
            int x = SECTION_TL.Width + i * SECTION_MC.Width + xOffset;
            PatchSection(data, source, SECTION_TC, origin, x, 0);
            for (int j = 0; j < rows; j++)
            {
                PatchSection(data, source, SECTION_MC, origin, x, SECTION_TC.Height - 1 + j * (SECTION_MC.Height - 1));
            }
            PatchSection(data, source, SECTION_BC, origin, x, SECTION_TC.Height - 1 + rows * (SECTION_MC.Height - 1));
        }
    }

    private void PatchGap(IAssetDataForMap data, Map source, Point origin, int rows, int start, int xOffset)
    {
        int x = SECTION_TL.Width + start * SECTION_MC.Width + xOffset;
        PatchSection(data, source, SECTION_BREAK_T, origin, x, 0);
        for (int j = 0; j < rows; j++)
        {
            PatchSection(
                data,
                source,
                SECTION_BREAK_M,
                origin,
                x,
                SECTION_BREAK_T.Height - 1 + j * (SECTION_BREAK_M.Height - 1)
            );
        }
        PatchSection(
            data,
            source,
            SECTION_BREAK_B,
            origin,
            x,
            SECTION_BREAK_T.Height - 1 + rows * (SECTION_BREAK_M.Height - 1)
        );
    }

    private static void PatchSection(IAssetDataForMap data, Map source, Rectangle section, Point origin, int x, int y)
    {
        data.PatchMap(
            source,
            section,
            new(origin.X + x, origin.Y + y, section.Width, section.Height),
            PatchMapMode.Overlay
        );
    }
}

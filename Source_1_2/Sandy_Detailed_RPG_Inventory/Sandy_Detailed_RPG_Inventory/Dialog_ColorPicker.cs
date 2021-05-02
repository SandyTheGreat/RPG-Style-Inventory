using System;
using RimWorld;
using UnityEngine;
using Verse;

//copy-paste from QualityColors.Dialog_ColorPicker, credit to the mod
//https://steamcommunity.com/sharedfiles/filedetails/?id=2420141361
namespace Sandy_Detailed_RPG_Inventory
{
    // Token: 0x02000003 RID: 3
    public class Dialog_ColorPicker : Window
    {
        // Token: 0x06000003 RID: 3 RVA: 0x000021E1 File Offset: 0x000003E1
        public Dialog_ColorPicker(Color initColor, Action<Color> setColor, string label)
        {
            this.color = initColor;
            this.doCloseButton = true;
            this.SetColor = setColor;
            this.SyncColor();
            this.label = label;
        }

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000004 RID: 4 RVA: 0x0000221A File Offset: 0x0000041A
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(350f, 590f);
            }
        }

        // Token: 0x06000005 RID: 5 RVA: 0x0000222B File Offset: 0x0000042B
        public override void PostClose()
        {
            this.SetColor(this.color);
            base.PostClose();
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002247 File Offset: 0x00000447
        public override void Close(bool doCloseSound = true)
        {
            this.SetColor(this.color);
            base.Close(doCloseSound);
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002264 File Offset: 0x00000464
        public void GenerateTextures()
        {
            this.tempStr = string.Format("{0},{1},{2}", this.color.r, this.color.g, this.color.b);
            Texture2D texture2D = new Texture2D(256, 256);
            for (int i = 0; i <= 256; i++)
            {
                for (int j = 0; j <= 256; j++)
                {
                    texture2D.SetPixel(i, j, Color.HSVToRGB((float)i / 256f, (float)j / 256f, this.value));
                }
            }
            texture2D.Apply();
            this.textures[0] = texture2D;
            texture2D = new Texture2D(15, 256);
            for (int k = 0; k <= 15; k++)
            {
                for (int l = 0; l <= 256; l++)
                {
                    texture2D.SetPixel(k, l, Color.HSVToRGB(this.hue, this.sat, (float)l / 256f));
                }
            }
            texture2D.Apply();
            this.textures[1] = texture2D;
            texture2D = new Texture2D(256, 16);
            for (int m = 0; m <= 15; m++)
            {
                for (int n = 0; n <= 256; n++)
                {
                    texture2D.SetPixel(n, m, new Color((float)n / 256f, this.color.g, this.color.b));
                }
            }
            texture2D.Apply();
            this.textures[2] = texture2D;
            texture2D = new Texture2D(256, 16);
            for (int num = 0; num <= 15; num++)
            {
                for (int num2 = 0; num2 <= 256; num2++)
                {
                    texture2D.SetPixel(num2, num, new Color(this.color.r, (float)num2 / 256f, this.color.b));
                }
            }
            texture2D.Apply();
            this.textures[3] = texture2D;
            texture2D = new Texture2D(256, 16);
            for (int num3 = 0; num3 <= 15; num3++)
            {
                for (int num4 = 0; num4 <= 256; num4++)
                {
                    texture2D.SetPixel(num4, num3, new Color(this.color.r, this.color.g, (float)num4 / 256f));
                }
            }
            texture2D.Apply();
            this.textures[4] = texture2D;
            texture2D = new Texture2D(256, 16);
            for (int num5 = 0; num5 <= 15; num5++)
            {
                for (int num6 = 0; num6 <= 256; num6++)
                {
                    texture2D.SetPixel(num6, num5, this.color);
                }
            }
            texture2D.Apply();
            this.textures[5] = texture2D;
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002590 File Offset: 0x00000790
        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = inRect.ContractedBy(10f);
            GUI.color = this.color;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(rect.x, rect.y, 256f, 30f), "RPG_Colors_Changing".Translate(label));
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            rect.y += 35f;
            Rect rect2 = new Rect(rect.x, rect.y, 256f, 256f);
            GUI.DrawTexture(rect2, this.textures[0], ScaleMode.StretchToFill);
            bool flag = Mouse.IsOver(rect2) && Event.current.isMouse && Event.current.button == 0;
            if (flag)
            {
                Vector2 mousePosition = Event.current.mousePosition;
                this.hue = (mousePosition.x - rect2.x) / 256f;
                this.sat = (rect2.yMax - mousePosition.y) / 256f;
                this.color = Color.HSVToRGB(this.hue, this.sat, this.value);
                this.GenerateTextures();
            }
            Rect rect3 = new Rect(rect2.xMax + 5f, rect.y, 15f, 256f);
            GUI.DrawTexture(rect3, this.textures[1], ScaleMode.StretchToFill);
            bool flag2 = Mouse.IsOver(rect3) && Event.current.isMouse && Event.current.button == 0;
            if (flag2)
            {
                this.value = (rect3.yMax - Event.current.mousePosition.y) / 256f;
                this.color = Color.HSVToRGB(this.hue, this.sat, this.value);
                this.GenerateTextures();
            }
            rect.y += 266f;
            rect3 = rect.TopPartPixels(30f);
            rect3.width /= 2f;
            this.tempStr = Widgets.TextField(rect3, this.tempStr);
            rect3.x = rect3.xMax;
            bool flag3 = Widgets.ButtonText(rect3, "RPG_Colors_Apply".Translate(), true, true, true);
            if (flag3)
            {
                try
                {
                    this.color = ParseHelper.ParseColor(this.tempStr);
                    this.SyncColor();
                }
                catch
                {
                }
            }
            rect.y += 40f;
            rect3 = new Rect(rect.x + 55f, rect.y, 256f, 15f);
            Widgets.Label(new Rect(rect.x, rect.y, 55f, 25f), "RPG_Colors_Red".Translate());
            GUI.DrawTexture(rect3, this.textures[2], ScaleMode.StretchToFill);
            bool flag4 = Mouse.IsOver(rect3) && Event.current.isMouse && Event.current.button == 0;
            if (flag4)
            {
                //Log.Message(string.Format("Mouse: {0}, Rect: {1}, Sub: {2}, r will be {3}", new object[]
                //{
                //    Event.current.mousePosition.x,
                //    rect3.x,
                //    Event.current.mousePosition.x - rect3.x,
                //    (Event.current.mousePosition.x - rect3.x) / 256f
                //}), false);
                this.color.r = (Event.current.mousePosition.x - rect3.x) / 256f;
                this.SyncColor();
            }
            rect.y += 40f;
            rect3 = new Rect(rect.x + 55f, rect.y, 256f, 15f);
            Widgets.Label(new Rect(rect.x, rect.y, 55f, 25f), "RPG_Colors_Green".Translate());
            GUI.DrawTexture(rect3, this.textures[3], ScaleMode.StretchToFill);
            bool flag5 = Mouse.IsOver(rect3) && Event.current.isMouse && Event.current.button == 0;
            if (flag5)
            {
                this.color.g = (Event.current.mousePosition.x - rect3.x) / 256f;
                this.SyncColor();
            }
            rect.y += 40f;
            rect3 = new Rect(rect.x + 55f, rect.y, 256f, 15f);
            Widgets.Label(new Rect(rect.x, rect.y, 55f, 25f), "RPG_Colors_Blue".Translate());
            GUI.DrawTexture(rect3, this.textures[4], ScaleMode.StretchToFill);
            bool flag6 = Mouse.IsOver(rect3) && Event.current.isMouse && Event.current.button == 0;
            if (flag6)
            {
                this.color.b = (Event.current.mousePosition.x - rect3.x) / 256f;
                this.SyncColor();
            }
            rect.y += 40f;
            rect3 = new Rect(rect.x + 55f, rect.y, 256f, 15f);
            Widgets.Label(new Rect(rect.x, rect.y, 55f, 25f), "RPG_Colors_Preview".Translate());
            GUI.DrawTexture(rect3, this.textures[5], ScaleMode.StretchToFill);
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002BA0 File Offset: 0x00000DA0
        private void SyncColor()
        {
            Color.RGBToHSV(this.color, out this.hue, out this.sat, out this.value);
            this.GenerateTextures();
        }

        // Token: 0x04000001 RID: 1
        //private readonly QualityCategory qual;
        private readonly string label;

        // Token: 0x04000002 RID: 2
        private readonly Texture2D[] textures = new Texture2D[6];

        // Token: 0x04000003 RID: 3
        private Color color;

        // Token: 0x04000004 RID: 4
        private float hue;

        // Token: 0x04000005 RID: 5
        private float sat;

        // Token: 0x04000006 RID: 6
        public Action<Color> SetColor;

        // Token: 0x04000007 RID: 7
        private string tempStr;

        // Token: 0x04000008 RID: 8
        private float value;
    }
}

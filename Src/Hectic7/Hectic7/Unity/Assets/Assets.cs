///
/// This is a generated code file
/// Expect to lose any changes you make
///
using UnityTools_4_6;
public class Assets
{
   public readonly static PrefabAsset MenuPrefab = new PrefabAsset(@"Menu");
   public readonly static PrefabAsset TimerPrefab = new PrefabAsset(@"Timer");
   public static Asset[] GetAssets() { return new Asset []{ MenuPrefab, TimerPrefab }; }
   public class Mars
   {
      public readonly static PrefabAsset Mar00Prefab = new PrefabAsset(@"Mars/Mar00");
      public readonly static PrefabAsset Mar01Prefab = new PrefabAsset(@"Mars/Mar01");
      public static Asset[] GetAssets() { return new Asset []{ Mar00Prefab, Mar01Prefab }; }
   }
   public class Tiles
   {
      public readonly static Texture2dAsset BoardsTexture2d = new Texture2dAsset(@"Tiles/Boards");
      public readonly static Texture2dAsset BubbleTexture2d = new Texture2dAsset(@"Tiles/Bubble");
      public readonly static Texture2dAsset BubbleGrassTexture2d = new Texture2dAsset(@"Tiles/BubbleGrass");
      public readonly static Texture2dAsset GrassLightTexture2d = new Texture2dAsset(@"Tiles/GrassLight");
      public readonly static Texture2dAsset Mar00BTexture2d = new Texture2dAsset(@"Tiles/Mar00B");
      public readonly static Texture2dAsset Mar01FTexture2d = new Texture2dAsset(@"Tiles/Mar01F");
      public static Asset[] GetAssets() { return new Asset []{ BoardsTexture2d, BubbleTexture2d, BubbleGrassTexture2d, GrassLightTexture2d, Mar00BTexture2d, Mar01FTexture2d }; }
      public class Materials
      {
         public readonly static MaterialAsset Boardsx8Material = new MaterialAsset(@"Tiles/Materials/Boardsx8");
         public readonly static MaterialAsset BubbleGrassx4Material = new MaterialAsset(@"Tiles/Materials/BubbleGrassx4");
         public readonly static MaterialAsset GrassLightx8Material = new MaterialAsset(@"Tiles/Materials/GrassLightx8");
         public readonly static MaterialAsset Mar00BMaterial = new MaterialAsset(@"Tiles/Materials/Mar00B");
         public readonly static MaterialAsset Mar01FMaterial = new MaterialAsset(@"Tiles/Materials/Mar01F");
         public static Asset[] GetAssets() { return new Asset []{ Boardsx8Material, BubbleGrassx4Material, GrassLightx8Material, Mar00BMaterial, Mar01FMaterial }; }
      }
   }
}

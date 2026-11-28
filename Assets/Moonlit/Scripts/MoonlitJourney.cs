using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public sealed partial class MoonlitLevel
    {
        float ExtensionStart => stage == 1 ? 180 : stage == 2 ? 157 : stage == 3 ? 156 : 149;

        // Fish jump in open water beside a firm bank. Crow fields have room to run
        // through or reverse course before the next traversal challenge.
        public readonly List<RiverAndSkyCreature> Wildlife = new List<RiverAndSkyCreature>();

        void PopulateWildlife()
        {
            switch (stage)
            {
                case 1:
                    Wildlife.Add(RiverAndSkyCreature.Fish(game, encounters, new Vector2(243, -1.2f), 3.3f, 1.4f));
                    break;
                case 2:
                    CrowPerch(154, 2.8f, 3.55f, 4.25f);
                    CrowPerch(524, 4.7f, 3.85f, 4.6f);
                    break;
                case 3:
                    Wildlife.Add(RiverAndSkyCreature.Fish(game, encounters, new Vector2(218.65f, -.75f), 3.9f, .64f));
                    Wildlife.Add(RiverAndSkyCreature.Fish(game, encounters, new Vector2(607.48f, 1.25f), 4.1f, .88f));
                    break;
                case 4:
                    CrowPerch(307.8f, 6, 3.35f, 3.4f);
                    break;
            }
        }

        void CrowPerch(float x, float floor, float height, float patrolRadius)
        {
            // A forked cutting lashed to the old bamboo fence. The crow's feet
            // rest on the top of its low, west-facing branch.
            var perch = new Vector2(x, floor + height);
            var basePoint = new Vector2(x + 1.0f, floor - .07f);
            var fork = new Vector2(x + .75f, floor + height * .64f);
            var branch = new Vector2(x + .63f, perch.y - .08f);
            var bark = new Color(.52f, .43f, .36f);
            var cut = new Color(.71f, .59f, .42f);
            Beam("Forked roost trunk", basePoint, fork, .32f, bark, decoration, -2);
            Beam("Forked roost trunk", fork, new Vector2(x + 1.32f, perch.y + .45f), .21f, bark, decoration, -2);
            Beam("Roost branch", fork, branch, .22f, bark, decoration, -2);
            Beam("Roost branch", branch, new Vector2(x - .65f, perch.y - .08f), .17f, cut, decoration, -2);
            Beam("Pruned twig", new Vector2(x + 1.10f, perch.y - .02f), new Vector2(x + 1.64f, perch.y + .08f), .11f, cut, decoration, -2);
            Wildlife.Add(RiverAndSkyCreature.Crow(game, encounters, perch, floor, patrolRadius));
        }

        // The later districts are authored in world coordinates just like the opening route.
        // Each dry landing provides a safe place to read the next crossing.
        void VillageJourney()
        {
            // Rice terraces: little rises followed by a dry irrigation bed.
            // Potter's lane: ambush from a cottage roof and the town's split-level steps.
            var lane = land[17]; Enemy(true, 281, 274, 285, lane, 3.35f);
            // The canal: walk from the stone jetty onto a moored raft, then across the fishing weir.
            var landing = land[22]; Enemy(false, 393, 389, 397, landing);
            // Orchard: stair up a long low hill and descend an old service path around its retaining wall.
            // Processional path: successive timber bays lead into a paved temple court.
            var court = land[31]; Enemy(false, 538, 534, 541, court);
            // Final causeway around the village pond, beneath the banyan's stone altar.
            Water(593, 610, -.75f); MooredSampan(602, 2.55f, BoatTint);
            Fence(585, 592); Fence(621, 627); HangLantern(618, 5.9f); Foliage(630, 0);

            Water(239, 252, -1.2f); Water(350, 382, -1.15f); Water(493, 517, -1.75f);
            Landmark("VillageCottage", 188, 0, 4.2f, VillageTint);
            Landmark("VillageCottage", 224, 1.25f, 3.7f, VillageTint);
            Landmark("VillageCottage", 281, .65f, 4.55f, VillageTint);
            Landmark("VillageGate", 320, 2.7f, 4.8f, VillageTint);
            MooredSampan(365, 2.8f, BoatTint);
            Landmark("YoungBanyan", 409, 1.65f, 6f, TreeTint);
            Landmark("YoungBanyan", 443, 4.25f, 6f, TreeTint);
            Landmark("VillageGate", 540, 1, 5.2f, VillageTint);
            Landmark("YoungBanyan", 639, EndFloor, 7.8f, TreeTint);
            Fence(183, 192); Fence(205, 212); Fence(252, 258); Fence(272, 277); Fence(315, 327);
            Fence(385, 398); Fence(419, 424); Fence(459, 469); Fence(518, 529); Fence(561, 570);
            Foliage(195, 0); Foliage(233, 0); Foliage(302, 0); Foliage(333, 0);
            Foliage(402, 0); Foliage(428, 0); Foliage(466, 0); Foliage(499, 0); Foliage(552, 0);
            HangLantern(266, 4); HangLantern(313.8f, 6.4f); HangLantern(347, 4); HangLantern(482, 4); HangLantern(558, 5);
        }

        void BambooJourney()
        {
            // The old charcoal kilns lead out over a high, wind-exposed canopy.
            // Bamboo waterwheel: successive lifted baskets climb back up the deep ravine.
            var ridge = land[12]; Enemy(false, 292, 288, 296, ridge);
            // The hermit's garden: a tall double-jump shelf, a sheltered glade, then a broken sky bridge.
            var glade = land[19]; Enemy(true, 428, 421, 432, glade, 3f);
            // Stone stair and split bamboo trunks. A long low clearing separates the two demanding ascents.
            // Climb out past the waterwheel and cross its upper sluice to the ridge shrine.
            Water(592, 616, -1.1f); Waterwheel(265, 1.1f, 2.85f); Waterwheel(605, 1.3f, 2.45f);
            HangLantern(576, 6f); Fence(583, 591); Fence(617, 625); Foliage(627, 0, true);

            Water(253, 281, -1.8f); Water(374, 402, -1.8f);
            Landmark("VillageCottage", 165, 2.8f, 3.8f, VillageTint);
            Landmark("YoungBanyan", 212, 7.15f, 5.5f, TreeTint);
            Landmark("VillageCottage", 347, 6.2f, 4.1f, VillageTint);
            Landmark("YoungBanyan", 423, 3.25f, 6.8f, TreeTint);
            Landmark("VillageGate", 626, EndFloor, 4.6f, VillageTint);
            foreach (float x in new[] { 179f, 204, 228, 248, 281, 303, 319, 334, 356, 373, 409, 436, 451, 471, 486, 525, 544, 560 }) Foliage(x, 0, true);
            Fence(229, 239); Fence(284, 297); Fence(343, 353); Fence(419, 432); Fence(518, 530);
            HangLantern(186, 10.7f); HangLantern(241, 8.7f); HangLantern(319, 6.8f); HangLantern(366, 10.8f); HangLantern(442, 7); HangLantern(540, 7);
        }

        void RiverJourney()
        {
            // Riverside warehouses: climb the far wharf and thread between piled landings.
            VineWall(173, 1.4f, 6.15f);
            // The ferry yards: go under a high jetty, up the boat ramp and across the empty berths.
            var yard = land[11]; Enemy(false, 247, 242, 251, yard);
            // Lantern procession: three extinguished paper stars lead back to an old floating bridge.
            BuildProcession(402, 418, 1.8f, new Vector2(344, 3.55f), new Vector2(370, 4.65f), new Vector2(395, 3.9f));
            VineWall(434, 1.8f, 6.15f);
            // Upper sluice walk: a lift and two short timber bridges deliver the player to the shrine beach.
            var bank = land[24]; Enemy(true, 530, 525, 533, bank, 2.8f);
            // The last sea wall guards an island of stone. The sampan passes beneath the gangway.
            VineWall(574, 1.4f, 6.15f);
            Water(588, 610, 1.25f); MooredSampan(600, 2.8f, BoatTint);
            FloatingLantern(591, 1.26f, 18); FloatingLantern(604, 1.26f, 19); FloatingLantern(607, 1.26f, 20);
            Landmark("VillageGate", 583, 7.55f, 5f, VillageTint);
            HangLantern(568, 5.5f); Fence(576, 587); Fence(613, 619); Foliage(622, 0);

            Water(199, 220, -.75f); Water(280, 306, -.65f); Water(375, 419, -.88f); Water(477.7f, 504.2f, 1.35f);
            Landmark("VillageCottage", 182, 7.55f, 4.3f, VillageTint); MooredSampan(210, 2.7f, BoatTint);
            Landmark("VillageCottage", 244, 2.05f, 4.1f, VillageTint); Landmark("VillageCottage", 273, 6.15f, 3.8f, VillageTint);
            MooredSampan(291, 3.4f, BoatTint); MooredSampan(412, 3.2f, BoatTint);
            Landmark("VillageGate", 427, 1.8f, 4.4f, VillageTint); Landmark("VillageCottage", 529, 2.55f, 3.75f, VillageTint);
            Landmark("YoungBanyan", 633, EndFloor, 6.6f, TreeTint);
            Fence(159, 169); Fence(193, 199); Fence(222, 233); Fence(257, 265); Fence(309, 319);
            Fence(345, 355); Fence(421, 430); Fence(450, 458); Fence(510, 518); Fence(539, 546);
            Foliage(169, 0); Foliage(238, 0); Foliage(315, 0); Foliage(364, 0); Foliage(446, 0); Foliage(471, 0);
            HangLantern(162, 5.6f); HangLantern(250, 6.3f); HangLantern(317, 6.8f); HangLantern(462, 8.6f); HangLantern(541, 5.5f);
            foreach (float x in new[] { 203f, 210, 217, 283, 289, 296, 303, 381, 387, 403, 411, 417, 481, 490, 500 }) FloatingLantern(x, -.75f, (int)x);
        }

        void HillJourney()
        {
            // A ruined ascending causeway. Every beacon stands on a broad, reversible landing.
            BuildProcession(225, 241, 2.8f, new Vector2(162, 4.1f), new Vector2(190, 6.35f), new Vector2(219, 4.9f));
            VineWall(254, 2.8f, 6.15f);
            // The longest break in the hill's backbone requires a dash before the last descent.
            var bluff = land[15]; Enemy(false, 286, 282, 289, bluff);
            // A counterweight lowers the hinged span onto a masonry shoulder below the two high steps.

            Fence(355, 363); HangLantern(380, 8.15f); Foliage(376, 0);
            Landmark("YoungBanyan", 154, 2, 5.8f, TreeTint); Landmark("VillageGate", 197, 4.25f, 4f, VillageTint);
            Landmark("YoungBanyan", 245, 2.8f, 7f, TreeTint); Landmark("YoungBanyan", 283, 8.7f, 5.9f, TreeTint);
            Fence(150, 157); Fence(173, 180); Fence(205, 212); Fence(243, 250);
            Foliage(166, 0); Foliage(185, 0); Foliage(256, 0); Foliage(309, 0);
            HangLantern(175, 7.4f); HangLantern(249, 7.1f); HangLantern(315, 8.05f);
        }

        void BuildProcession(float start, float end, float top, params Vector2[] lamps)
        {
            var bridge = new GameObject("Lantern Procession Bridge");
            bridge.transform.SetParent(terrain);
            bridge.transform.position = new Vector2((start + end) * .5f, top);
            const int pieces = 4;
            float width = (end - start) / pieces;
            for (int i = 0; i < pieces; i++)
            {
                float center = start + (i + .5f) * width;
                var deck = BambooPlatform(center, top, width + .04f, suspended: true);
                SupportDeck(center, top, width, false);
                deck.transform.SetParent(bridge.transform, true);
            }
            Procession = LanternProcession.Create(game, encounters, lamps, bridge);
        }

        string JourneyArea(float x)
        {
            switch (stage)
            {
                case 1: return x < 269 ? "Ruộng bậc thang" : x < 352 ? "Ngõ lò gốm" : x < 400 ? "Bến cất vó" : x < 494 ? "Vườn cau trên đồi" : x < 582 ? "Sân rước đèn" : "Hồ Bán Nguyệt";
                case 2: return x < 242 ? "Đường vào lò than" : x < 324 ? "Guồng nước bên suối" : x < 376 ? "Vườn sau am" : x < 436 ? "Lối qua cổng trúc" : x < 488 ? "Dốc hai bụi trúc" : x < 568 ? "Bậc tre trên sườn núi" : "Miếu đầu nguồn";
                case 3: return x < 237 ? "Kho thóc trên bờ" : x < 325 ? "Xưởng đóng thuyền" : x < 419 ? "Bến rước đèn" : x < 466 ? "Đường đê" : x < 507 ? "Cống nước" : x < 561 ? "Bãi đền ven sông" : "Cồn thả đèn";
                default: return x < 241 ? "Lối lên đồi" : x < 278 ? "Vách rễ đa" : x < 320 ? "Sống đá gãy" : x < 383 ? "Cửa rừng thiêng" : "Gốc đa cổ thụ";
            }
        }

        string JourneyHint(float x)
        {
            if (stage == 1 && x > 235 && x < 244) return $"Thấy nước gợn thành vòng thì tránh ra hoặc bấm {game.Controls.Label(GameAction.Lantern)} để rọi đèn.";
            if (Procession && !Procession.Complete)
            {
                float lamp = Procession.NextLight.x;
                if (Mathf.Abs(x - lamp) < 6) return $"Đứng gần đèn ông sao rồi bấm {game.Controls.Label(GameAction.Lantern)} để thắp.";
                if ((stage == 3 && x > 392 && x < 407) || (stage == 4 && x > 216 && x < 230))
                    return "Đèn tiếp theo ở bên " + (lamp < x ? "trái." : "phải.");
            }
            if (stage == 2 && x > 166 && x < 174) return "Đợi gần tới đỉnh cú nhảy rồi nhảy lần nữa để lên bờ cao.";
            if (stage == 3 && x > 166 && x < 174) return $"Áp sát vách, giữ phím hướng về vách và {game.Controls.Label(GameAction.Jump)} để leo.";
            if (stage == 4 && x > 261 && x < 270) return "Nhảy đôi rồi lướt qua khe đá.";
            return "";
        }
    }
}

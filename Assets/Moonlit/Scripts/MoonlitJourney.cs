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
            Ground(180, 194, 0); Ground(194, 202, .95f); Ground(202, 214, 1.9f);
            Ground(216.5f, 230, 1.25f); Ground(230, 242, .35f);
            BambooPlatform(246.1f, .95f, 4.6f); Ground(250, 269, .65f);
            Checkpoint(261, .65f, 3);
            // Potter's lane: ambush from a cottage roof and the town's split-level steps.
            var lane = Ground(269, 287, .65f); Enemy(true, 281, 274, 285, lane, 3.35f);
            Ground(287, 294, 1.85f); Ground(294, 305, 3.15f);
            Platform(309, 3.5f, 4.1f); Ground(313, 329, 2.7f);
            Ground(329, 337, 1.4f); Ground(337, 353, .35f);
            // The canal: walk from the stone jetty onto a moored raft, then across the fishing weir.
            BambooPlatform(357, .75f, 4.6f, new Vector2(.5f, 0));
            BambooPlatform(363.3f, 1.1f, 4.4f); BambooPlatform(369.5f, 1.5f, 4.6f);
            BambooPlatform(375.8f, .95f, 4.8f, new Vector2(.5f, 0));
            var landing = Ground(380, 400, .65f); Checkpoint(384, .65f, 4); Enemy(false, 393, 389, 397, landing);
            // Orchard: stair up a long low hill and descend an old service path around its retaining wall.
            Ground(400, 413, 1.65f); Ground(413, 425, 2.85f); Ground(425, 435, 4.55f);
            Ground(438, 449, 4.25f); Platform(453, 3.8f, 4.1f); Ground(457, 470, 2.1f);
            Ground(470, 482, .65f); Ground(482, 495, 0); Checkpoint(488, 0, 5);
            // Processional path: successive timber bays lead into a paved temple court.
            BambooPlatform(499, .7f, 4.5f); Platform(505, 1.55f, 4.1f, null, true);
            Platform(511.2f, 2.45f, 4f, null, true); Ground(515, 531, 2);
            var court = Ground(531, 549, 1); Enemy(false, 538, 534, 541, court);
            Ground(549, 557, 1.9f); Ground(557, 583, 1); Checkpoint(577, 1, 6);
            // Final causeway around the village pond, beneath the banyan's stone altar.
            Ground(583, 594, 1.85f); BambooPlatform(598.1f, 2.15f, 4.5f);
            BambooPlatform(604.4f, 2.6f, 4.6f); Ground(608.5f, 620, 1.9f);
            Ground(620, 649, EndFloor);
            Water(593, 610, -.75f); Landmark("RiverSampan", 602, -.85f, 2.55f, BoatTint);
            Fence(585, 592); Fence(621, 627); HangLantern(618, 5.9f); Foliage(630, 0);
            Trail(578, 2.55f, 590, 3.4f, 598, 3.7f, 604, 4.15f, 614, 3.45f, 630, 2.55f);

            Water(239, 252, -1.2f); Water(350, 382, -1.15f); Water(493, 517, -1.75f);
            Landmark("VillageCottage", 188, 0, 4.2f, VillageTint);
            Landmark("VillageCottage", 224, 1.25f, 3.7f, VillageTint);
            Landmark("VillageCottage", 281, .65f, 4.55f, VillageTint);
            Landmark("VillageGate", 320, 2.7f, 4.8f, VillageTint);
            Landmark("RiverSampan", 365, -1.2f, 2.8f, BoatTint);
            Landmark("YoungBanyan", 409, 1.65f, 6f, TreeTint);
            Landmark("YoungBanyan", 443, 4.25f, 6f, TreeTint);
            Landmark("VillageGate", 540, 1, 5.2f, VillageTint);
            Landmark("YoungBanyan", 639, EndFloor, 7.8f, TreeTint);
            Fence(183, 192); Fence(205, 212); Fence(252, 258); Fence(272, 277); Fence(315, 327);
            Fence(385, 398); Fence(419, 424); Fence(459, 469); Fence(518, 529); Fence(561, 570);
            Foliage(195, 0); Foliage(233, 0); Foliage(302, 0); Foliage(333, 0);
            Foliage(402, 0); Foliage(428, 0); Foliage(466, 0); Foliage(499, 0); Foliage(552, 0);
            HangLantern(266, 4); HangLantern(313.8f, 6.4f); HangLantern(347, 4); HangLantern(482, 4); HangLantern(558, 5);
            Trail(185, 1.45f, 197, 2.4f, 208, 3.4f, 215, 3.55f, 239, 1.8f, 246, 2.5f, 258, 2.1f, 275, 2.1f, 291, 3.3f, 301, 4.7f,
                  309, 5.05f, 324, 4.1f, 345, 1.8f, 357, 2.2f, 363.3f, 2.6f, 369.5f, 3f, 376, 2.5f, 402, 3.2f,
                  418, 4.4f, 430, 6.1f, 436.5f, 6f, 453, 5.3f, 469, 3.6f, 481, 2.1f, 499, 2.2f, 505, 3.1f, 511, 4f, 528, 3.5f, 553, 3.5f, 568, 2.5f);
        }

        void BambooJourney()
        {
            // The old charcoal kilns lead out over a high, wind-exposed canopy.
            Ground(157, 172, 2.8f); Ground(172, 187, 6.65f);
            BambooPlatform(192.2f, 6.9f, 4.4f); BambooPlatform(200.1f, 7.7f, 4.5f);
            Ground(206, 222, 7.15f); Ground(225.5f, 242, 4.8f); Checkpoint(234, 4.8f, 3);
            // Bamboo waterwheel: successive lifted baskets climb back up the deep ravine.
            Ground(242, 255, 3); Platform(259.2f, 3.8f, 4.6f, new Vector2(0, .6f));
            Platform(266.3f, 5.4f, 4.5f); Platform(274, 6.65f, 4.7f, new Vector2(0, .55f));
            var ridge = Ground(280, 300, 6.7f); Enemy(false, 292, 288, 296, ridge);
            Ground(300, 309, 5); Ground(309, 324, 3); Ground(324, 339, 2);
            // The hermit's garden: a tall double-jump shelf, a sheltered glade, then a broken sky bridge.
            Ground(339, 354, 6.2f); Ground(357, 375, 6.4f); Checkpoint(368, 6.4f, 4);
            Platform(380.2f, 7.2f, 4.4f, null, true); Platform(387.5f, 7.65f, 4.6f, null, true);
            BambooPlatform(395.8f, 6.9f, 4.6f); Ground(401, 416, 5.35f);
            var glade = Ground(416, 436, 3.25f); Enemy(true, 428, 421, 432, glade, 3f);
            // Stone stair and split bamboo trunks. A long low clearing separates the two demanding ascents.
            Ground(436, 450, 3.25f); Ground(450, 460, 6.8f); Ground(460, 477, 7.5f);
            Ground(477, 488, 5.1f); Checkpoint(483, 5.1f, 5);
            BambooPlatform(493, 5.4f, 4.5f); Platform(500.4f, 6.7f, 4.7f); BambooPlatform(508.5f, 6.15f, 4.5f);
            Ground(515, 532, 4.7f); Ground(532, 546, 3.15f); Ground(546, 568, 2); Checkpoint(556, 2, 6);
            // Climb out past the waterwheel and cross its upper sluice to the ridge shrine.
            Ground(568, 580, 2); Ground(580, 594, 5.85f);
            Platform(599.4f, 6.65f, 4.8f, new Vector2(0, .55f));
            BambooPlatform(607.6f, 6.4f, 4.7f); Ground(614, 640, EndFloor);
            Water(592, 616, -1.1f); Waterwheel(265, 1.1f, 2.85f); Waterwheel(605, 1.3f, 2.45f);
            HangLantern(576, 6f); Fence(583, 591); Fence(617, 625); Foliage(627, 0, true);
            Trail(565, 3.5f, 579.3f, 5f, 583, 7.4f, 593, 7.35f, 599.4f, 8.65f, 607.6f, 8f, 623, 7.55f);

            Water(253, 281, -1.8f); Water(374, 402, -1.8f);
            Landmark("VillageCottage", 165, 2.8f, 3.8f, VillageTint);
            Landmark("YoungBanyan", 212, 7.15f, 5.5f, TreeTint);
            Landmark("VillageCottage", 347, 6.2f, 4.1f, VillageTint);
            Landmark("YoungBanyan", 423, 3.25f, 6.8f, TreeTint);
            Landmark("VillageGate", 626, EndFloor, 4.6f, VillageTint);
            foreach (float x in new[] { 179f, 204, 228, 248, 281, 303, 319, 334, 356, 373, 409, 436, 451, 471, 486, 525, 544, 560 }) Foliage(x, 0, true);
            Fence(229, 239); Fence(284, 297); Fence(343, 353); Fence(419, 432); Fence(518, 530);
            HangLantern(186, 10.7f); HangLantern(241, 8.7f); HangLantern(319, 6.8f); HangLantern(366, 10.8f); HangLantern(442, 7); HangLantern(540, 7);
            Trail(166, 4.25f, 171, 5.8f, 175, 8.2f, 192, 8.5f, 200, 9.3f, 216, 8.7f, 238, 6.4f, 252, 4.6f, 259, 5.8f, 266, 7f, 274, 8.6f,
                  297, 8.25f, 306, 6.45f, 325, 3.6f, 338.5f, 5.3f, 344, 7.75f, 355.5f, 8.3f, 372, 7.9f, 380, 8.75f, 387, 9.15f, 396, 8.4f,
                  412, 6.9f, 432, 4.85f, 449.5f, 6f, 455, 8.35f, 474, 9.05f, 493, 7f, 500, 8.2f, 508, 7.85f, 526, 6.15f, 546, 4.7f, 553, 3.55f);
        }

        void RiverJourney()
        {
            // Riverside warehouses: climb the far wharf and thread between piled landings.
            Ground(156, 173, 1.4f); Ground(173, 188, 7.55f); VineWall(173, 1.4f, 6.15f);
            Ground(188, 201, 5.75f); BambooPlatform(206, 4.6f, 4.6f); BambooPlatform(213.4f, 3.8f, 4.8f, new Vector2(.65f, 0));
            Ground(219, 237, 2.05f); Checkpoint(229, 2.05f, 3);
            // The ferry yards: go under a high jetty, up the boat ramp and across the empty berths.
            var yard = Ground(237, 256, 2.05f); Enemy(false, 247, 242, 251, yard);
            Ground(256, 268, 4.1f); Ground(268, 281, 6.15f);
            BambooPlatform(286, 5.7f, 4.8f); BambooPlatform(293.7f, 4.5f, 4.8f, new Vector2(.65f, 0));
            BambooPlatform(301, 3.55f, 4.8f); Ground(306, 324, 2.45f); Ground(324, 341, 1.45f);
            Checkpoint(336, 1.45f, 4);
            // Lantern procession: three extinguished paper stars lead back to an old floating bridge.
            Ground(341, 357, 1.45f); Ground(357, 376, 2.55f); Ground(379, 402, 1.8f);
            BuildProcession(402, 418, 1.8f, new Vector2(344, 3.55f), new Vector2(370, 4.65f), new Vector2(395, 3.9f));
            Ground(418, 434, 1.8f); Ground(434, 447, 7.95f); VineWall(434, 1.8f, 6.15f);
            Ground(447, 461, 5.9f); Ground(461, 478, 4.1f); Checkpoint(472, 4.1f, 5);
            // Upper sluice walk: a lift and two short timber bridges deliver the player to the shrine beach.
            Platform(483.1f, 4.9f, 4.7f, new Vector2(0, .55f)); Platform(490.5f, 6f, 4.6f, null, true);
            BambooPlatform(498.4f, 5.2f, 4.9f); Ground(504, 521, 4.35f);
            var bank = Ground(521, 536, 2.55f); Enemy(true, 530, 525, 533, bank, 2.8f);
            Ground(536, 561, 1.4f); Checkpoint(552, 1.4f, 6);
            // The last sea wall guards an island of stone. The sampan passes beneath the gangway.
            Ground(561, 574, 1.4f); Ground(574, 589, 7.55f); VineWall(574, 1.4f, 6.15f);
            BambooPlatform(594.3f, 6.7f, 4.9f); BambooPlatform(602, 5.7f, 5, new Vector2(.65f, 0));
            Ground(608, 620, 4.25f); Ground(620, 643, EndFloor);
            Water(588, 610, 1.25f); Landmark("RiverSampan", 600, 1.12f, 2.8f, BoatTint);
            FloatingLantern(591, 1.26f, 18); FloatingLantern(604, 1.26f, 19); FloatingLantern(607, 1.26f, 20);
            Landmark("VillageGate", 583, 7.55f, 5f, VillageTint);
            HangLantern(568, 5.5f); Fence(576, 587); Fence(613, 619); Foliage(622, 0);
            Trail(563, 3f, 573.6f, 4.15f, 573.6f, 6.3f, 580, 9.1f, 594, 8.25f, 602, 7.25f, 615, 5.8f, 626, 3.65f);

            Water(199, 220, -.75f); Water(280, 306, -.65f); Water(375, 419, -.88f); Water(478, 505, -1.4f);
            Landmark("VillageCottage", 182, 7.55f, 4.3f, VillageTint); Landmark("RiverSampan", 210, -.78f, 2.7f, BoatTint);
            Landmark("VillageCottage", 244, 2.05f, 4.1f, VillageTint); Landmark("VillageCottage", 273, 6.15f, 3.8f, VillageTint);
            Landmark("RiverSampan", 291, -.75f, 3.4f, BoatTint); Landmark("RiverSampan", 412, -.9f, 3.2f, BoatTint);
            Landmark("VillageGate", 427, 1.8f, 4.4f, VillageTint); Landmark("VillageCottage", 529, 2.55f, 3.75f, VillageTint);
            Landmark("YoungBanyan", 633, EndFloor, 6.6f, TreeTint);
            Fence(159, 169); Fence(193, 199); Fence(222, 233); Fence(257, 265); Fence(309, 319);
            Fence(345, 355); Fence(421, 430); Fence(450, 458); Fence(510, 518); Fence(539, 546);
            Foliage(169, 0); Foliage(238, 0); Foliage(315, 0); Foliage(364, 0); Foliage(446, 0); Foliage(471, 0);
            HangLantern(162, 5.6f); HangLantern(250, 6.3f); HangLantern(317, 6.8f); HangLantern(462, 8.6f); HangLantern(541, 5.5f);
            foreach (float x in new[] { 203f, 210, 217, 283, 289, 296, 303, 381, 387, 403, 411, 417, 481, 490, 500 }) FloatingLantern(x, -.75f, (int)x);
            Trail(165, 3f, 172.5f, 4.1f, 172.5f, 6.35f, 178, 9.1f, 196, 7.25f, 206, 6.1f, 213, 5.3f, 234, 3.6f, 260, 5.7f, 277, 7.7f, 286, 7.25f,
                  294, 6.1f, 301, 5.1f, 323, 4f, 348, 3f, 360, 4.1f, 377.5f, 4.55f, 384, 3.35f, 405, 3.35f, 413, 3.35f, 425, 3.4f,
                  433.6f, 4.5f, 433.6f, 6.7f, 440, 9.5f, 456, 7.45f, 475, 5.6f, 483, 6.9f, 491, 7.55f, 498, 6.8f, 513, 5.9f, 539, 3f);
        }

        void HillJourney()
        {
            // A ruined ascending causeway. Every beacon stands on a broad, reversible landing.
            Ground(149, 170, 2); Ground(170, 182, 3.5f); Ground(182, 199, 4.25f);
            Ground(202, 213, 3.65f); Ground(213, 225, 2.8f);
            BuildProcession(225, 241, 2.8f, new Vector2(162, 4.1f), new Vector2(190, 6.35f), new Vector2(219, 4.9f));
            Ground(241, 254, 2.8f); Ground(254, 268, 8.95f); VineWall(254, 2.8f, 6.15f);
            Checkpoint(261, 8.95f, 4);
            // The longest break in the hill's backbone requires a dash before the last descent.
            var bluff = Ground(278.6f, 292, 8.7f); Enemy(false, 286, 282, 289, bluff);
            Platform(297.2f, 7.85f, 4.6f, null, true); Ground(303, 313, 6); Ground(313, 320, 4.15f);
            Ground(320, 331, 2); Checkpoint(324, 2, 5);
            // The sacred wood begins with a final root wall and crosses the two terraces above the clearing.
            Ground(331, 343, 8.2f); VineWall(331, 2, 6.2f);
            Platform(348.1f, 8.85f, 4.8f, null, true); Ground(354, 365, 7.4f);
            BambooPlatform(370, 6.5f, 4.75f); Ground(375, 383, 4.15f);
            Ground(383, 417, EndFloor); Checkpoint(385, EndFloor, 6);
            Landmark("YoungBanyan", 337, 8.2f, 6.25f, TreeTint);
            Fence(355, 363); HangLantern(380, 8.15f); Foliage(376, 0);
            Trail(328, 3.6f, 330.6f, 4.65f, 330.6f, 6.95f, 335, 9.8f, 348, 10.5f, 360, 9f, 370, 8.05f, 379, 5.75f);
            Landmark("YoungBanyan", 154, 2, 5.8f, TreeTint); Landmark("VillageGate", 197, 4.25f, 4f, VillageTint);
            Landmark("YoungBanyan", 245, 2.8f, 7f, TreeTint); Landmark("YoungBanyan", 283, 8.7f, 5.9f, TreeTint);
            Fence(150, 157); Fence(173, 180); Fence(205, 212); Fence(243, 250);
            Foliage(166, 0); Foliage(185, 0); Foliage(256, 0); Foliage(309, 0);
            HangLantern(175, 7.4f); HangLantern(249, 7.1f); HangLantern(315, 8.05f);
            Trail(157, 3.5f, 173, 5.1f, 183, 5.8f, 200.6f, 6.3f, 207, 5.2f, 216, 4.4f, 229, 4.3f, 237, 4.3f, 250, 4.4f,
                  253.6f, 5.4f, 253.6f, 7.6f, 258, 10.5f, 267.5f, 10.8f, 271, 12.1f, 275, 12f, 280, 10.4f,
                  297, 9.5f, 309, 7.5f, 317, 5.7f);
        }

        void BuildProcession(float start, float end, float top, params Vector2[] lamps)
        {
            var bridge = new GameObject("Cầu rước đèn");
            bridge.transform.SetParent(terrain);
            bridge.transform.position = new Vector2((start + end) * .5f, top);
            const int pieces = 4;
            float width = (end - start) / pieces;
            for (int i = 0; i < pieces; i++)
            {
                var deck = BambooPlatform(start + (i + .5f) * width, top, width + .04f);
                deck.transform.SetParent(bridge.transform, true);
            }
            Procession = LanternProcession.Create(game, encounters, lamps, bridge);
        }

        string JourneyArea(float x)
        {
            switch (stage)
            {
                case 1: return x < 269 ? "Ruộng bậc ven làng" : x < 352 ? "Ngõ lò gốm" : x < 400 ? "Bến vó" : x < 494 ? "Vườn cau trên đồi" : x < 582 ? "Sân lễ rước trăng" : "Hồ bán nguyệt";
                case 2: return x < 242 ? "Lối lò than" : x < 324 ? "Guồng nước trúc" : x < 376 ? "Vườn am cổ" : x < 436 ? "Nhịp cầu ngang mây" : x < 488 ? "Dốc song trúc" : x < 568 ? "Bậc trúc trên cao" : "Miếu đầu nguồn";
                case 3: return x < 237 ? "Kho thóc bờ cao" : x < 325 ? "Xưởng thuyền" : x < 419 ? "Bến rước sao" : x < 478 ? "Cống nước cổ" : x < 561 ? "Bãi đền ven sông" : "Đảo hoa đăng";
                default: return x < 241 ? "Đường đèn lên đồi" : x < 278 ? "Vách rễ cổ" : x < 320 ? "Sống đá gãy" : x < 383 ? "Cửa rừng thiêng" : "Tán Đa thần";
            }
        }

        string JourneyHint(float x)
        {
            if (stage == 1 && x > 235 && x < 244) return "Vòng nước báo cá sắp nhảy   ·   J / X bùng sáng";
            if (Procession && !Procession.Complete)
            {
                float lamp = Procession.NextLight.x;
                if (Mathf.Abs(x - lamp) < 6) return "J / X   ·   Thắp đèn dẫn lối";
                if ((stage == 3 && x > 392 && x < 407) || (stage == 4 && x > 216 && x < 230))
                    return "Thắp đủ ba đèn để mở cầu   ·   Cột tiếp theo ở phía " + (lamp < x ? "trái" : "phải");
            }
            if (stage == 2 && x > 166 && x < 174) return "Nhảy lần hai gần đỉnh để lên bờ tre cao";
            if (stage == 3 && x > 166 && x < 174) return "Giữ hướng vào dây leo và giữ Space";
            if (stage == 4 && x > 261 && x < 270) return "Nhảy đôi và lướt trên không qua sống đá";
            return "";
        }
    }
}

# Cá chép và quạ trong Cuội Tìm Trăng

Hai bộ sprite được tạo cho trò chơi ngày 24/11/2026 bằng OpenAI ImageGen trong Codex, trực tiếp trên nền trong suốt. Mỗi file PNG nguồn chứa bốn tư thế. File nhập Unity đi kèm lưu vùng cắt, điểm xoay và tỉ lệ riêng của từng loài. Hai lần tạo dùng các mô tả văn bản dưới đây.

## Cá chép

[BronzeCarp.png](../Assets/Moonlit/Resources/BronzeCarp.png) gồm bơi ngang, ngẩng đầu lấy đà, bật khỏi nước và chúi đầu lặn xuống. Sprite dùng vảy màu đồng với viền lưng xanh ngọc để đầu và đuôi tách khỏi mặt nước.

> A single original production sprite sheet for a Vietnamese folktale side-scrolling game. TRANSPARENT canvas with exactly FOUR individual isolated gold-bronze Vietnamese river carp poses in a precise 2 by 2 grid. Equal size square cells, large transparent gutters, every sprite entirely within its own cell, all fish facing right in side profile. Top left: idle horizontal carp seen from side, fan tail curved left, clear bronze scales and a single alert dark eye; top right: head tilted upward about 30 degrees, pectoral fins flared in anticipation; bottom left: leaping curved carp facing upper right about 55 degrees, tail sweeping; bottom right: curving downward to lower right, fins folded to dive. These are the SAME fish in FOUR poses. Rich muted terracotta and ochre scales, teal/cyan rim light along back and pale tan belly, very dark indigo edge. Crisp authentic hand-clustered 16-bit pixel art with chunky readable silhouette for 48px assets, limited flat pixel clusters; actual transparent exterior. Keep fin/tail edges clear. No water, no bubbles, no ground, no shadow, no sheet guides, no labels, no text or ornament.

## Quạ

[MidnightCrow.png](../Assets/Moonlit/Resources/MidnightCrow.png) gồm đậu khép cánh, giương cánh chuẩn bị, bay sà và vỗ cánh trở về. Ở hai khung đậu, bàn chân được căn vào cành trong màn chơi.

> A production character sprite sheet featuring ONE original glossy midnight-blue jungle crow from Vietnamese folklore in FOUR isolated animation poses on transparent background, precise 2 by 2 grid of equal square cells, centered in each cell with generous transparent gutters. Side view, always facing RIGHT. Top left perched crow with folded deep navy wings, beak and alert amber eye, feet visible; top right crow still perched, breast lowered, wings raised in a strong readable V about to launch, both feet on same baseline; bottom left in flight, wings completely extended to glide, body tipped down toward the right, feet tucked; bottom right in flight, wings flapping upward, body leveling and tail fanned. All four images same crow. Crisp folk-tale pixel art, clear silhouette, a few intentionally large clusters of midnight navy highlights, muted sapphire and peacock teal feather rim, warm ochre beak edge, amber eye. Must read at 40-60px tall. No perch, no branch, no feet shadows, no background, no letters, no bars or grids. Transparent α in gutters and around sprites. All four complete, no part of any wing touches cell boundary.

## Tiếng động và hiệu ứng

Năm file WAV mono 24 kHz được tổng hợp trong project bằng [ScoreMoonlit.py](../Tools/ScoreMoonlit.py). Các hiệu ứng dùng dao động, nhiễu và đường bao âm lượng do chương trình tính cho từng tiếng.

| Âm thanh | Cách tạo |
|---|---|
| [Bọt nước](../Assets/Moonlit/Resources/MoonAudio/sfx_carpBubbles.wav) | Ba tiếng bụp ngắn có cao độ tăng, báo cá đang lấy đà |
| [Cá bật lên](../Assets/Moonlit/Resources/MoonAudio/sfx_carpLeap.wav) | Tiếng nước bị xé, cao độ vuốt lên và một giọt bắn |
| [Cá rơi xuống](../Assets/Moonlit/Resources/MoonAudio/sfx_carpSplash.wav) | Hai lớp nước vỡ có cao độ hạ, theo sau bằng một giọt |
| [Quạ kêu](../Assets/Moonlit/Resources/MoonAudio/sfx_crowCall.wav) | Hai âm khàn tổng hợp bằng điều chế pha và các họa âm lẻ |
| [Quạ vỗ cánh](../Assets/Moonlit/Resources/MoonAudio/sfx_crowFlight.wav) | Ba nhịp gió ngắn, nhỏ dần |

[WetlandCreatureArt.cs](../Assets/Moonlit/Scripts/WetlandCreatureArt.cs) vẽ gợn sóng và bọt lúc cá chuẩn bị nhảy, dấu hướng hạ cánh của quạ và các đốm sáng lúc sinh vật bị choáng. Chuyển động theo địa hình, thời gian báo trước và va chạm được định nghĩa trong [RiverAndSkyCreature.cs](../Assets/Moonlit/Scripts/RiverAndSkyCreature.cs).

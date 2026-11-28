# Bốn đường vượt và chuyện trăng vỡ

Bản 2.3.0 ngày 28/11/2026. Mỗi chặng bổ sung một tình huống trên tuyến chơi chính.

| Địa điểm | Việc người chơi làm | Phản hồi |
|---|---|---|
| Ngõ lò gốm, Bờ Tre Làng Cuội | Đẩy thùng lên dấu bàn cân, nhảy qua thùng rồi đi trên cầu mới hạ. Một rãnh chông ngắn trước đó cho phép làm quen với mối nguy. | Thùng chốt vào đúng vị trí. Hai dây luồn qua khoen trên xà, đầu dưới bắt vào đai trên sàn tre. Khi cầu hạ ngang bờ, Cuội có thể bước qua. |
| Lối qua cổng trúc, Rừng Trúc | Đi theo cầu thấp tới cổng rồi quay lại giàn tre. Nhảy lên hai bàn đạp ở hai đầu ngọn trúc, theo thứ tự tùy ý. | Mỗi chuông sáng một đèn cạnh cổng. Khi cả hai đèn sáng, Cuội có thể xuống cầu thấp và đi tiếp. |
| Cống nước, Sông Đèn | Dùng nút tương tác trên bè hoặc cạnh trụ đèn của một trong hai bến. Có thể gọi bè về sau khi rơi. Muốn lấy hạt giữa sông, Cuội phải canh lúc nhảy và trở xuống sàn đang chạy. | Hai đầu cáp bắt vào hai giá tre. Con trượt chạy trên cáp và dây treo giữa bè đi theo con trượt. Đèn bến báo vị trí bè. |
| Miếu bến đò, Sông Đèn | Gọi cầu tại miếu bằng nút tương tác hoặc rọi đèn. | Bốn nhịp tre lần lượt trồi lên từ dưới nước, nghiêng nhẹ rồi vào khớp. Từng nhịp đỡ được Cuội khi nằm ngang. |
| Bến rước đèn, Sông Đèn và lối qua khe ở Đồi Gốc Đa | Rọi sáng đủ ba đèn ông sao dọc đường. | Ngọn cuối sáng lên gọi bốn nhịp cầu trồi từ dưới khe. Ván ngả theo sóng, lần lượt bắt vào mố và đỡ được Cuội. Cọc dẫn giữ nguyên vị trí dưới sàn ở cả hai nơi. |
| Cầu gỗ trên Đồi Gốc Đa | Đẩy thùng lên bàn cân để hạ cầu. Qua cầu rồi nhảy đôi lên hai bậc đá. | Đầu cầu xoay quanh bản lề rồi chạm mố đá. Thùng giữ chốt đến khi chơi lại màn. |

## Câu chuyện và vật phẩm

Cảnh mở diễn ra khi bắt đầu Bờ Tre Làng Cuội. Trên Cung Trăng, Cây Đa vung rễ đập vào đĩa trăng. Bốn mảnh lớn tách khỏi nhau và rơi về bốn vùng. Mỗi màn có một mảnh riêng ở đích. Mỗi màn có đúng mười hạt trăng, giấu ở những chỗ như gốc đa, giá tre và mép bến. Cuội lấy mảnh lớn ngay khi chạm vào. Hạt trăng dành cho những ai thích tìm đường vòng. Mỗi màn ghi riêng số hạt nhặt được nhiều nhất trong một lượt.

Sau khi cây dịu lại, cùng bốn hình mảnh vỡ ấy bay ngược lên, ghép thành đĩa trăng. Cuội cầm đèn ngồi trên cành cùng cây trở về. Hai hoạt cảnh tự chuyển tiếp hoặc có thể bấm Bỏ qua, Space hay Esc. Chơi lại từ checkpoint đưa Cuội trở vào đường chơi.

## Chất liệu, âm thanh và điểm tựa

Bộ ván gỗ giữ hai đầu chống và ghép mặt ván ở giữa. Bè dài giữ hai mũi, ghép tiếp thân bè. Các cột tre, thành cầu và móng đá lặp từng đoạn theo mật độ điểm ảnh của mẫu gốc. Đất và đường mép lấy mẫu theo cùng một tỉ lệ ngang/dọc. Các mô-đun cắt từ hai atlas đã có của game. [ForestExtras](../Assets/Moonlit/Art/ForestExtras.png) · [Nguồn địa hình](terrain-sources.json)

Vị trí rễ dựa trên mép pixel có màu trong ảnh cây. Vệt tối ở chân rễ nằm trên mặt đất. Dây cáp của sàn nâng luồn qua hai khoen trên xà và hai khoen trên mặt sàn. Khoen là họa tiết pixel 13 × 20 do project tạo trong LiftSuspension.cs. Chuông đồng dưới hai bàn đạp dùng hình pixel vẽ riêng trong BambooChimeGate.cs. Tiếng lá trúc, nước chảy, chốt gỗ và cầu chạm mố được tổng hợp riêng bằng NumPy, không dùng bản thu ngoài. [Mã tạo âm thanh](../Tools/ScoreMoonlit.py)

## Nguồn

Phông toàn cảnh do OpenAI ImageGen tích hợp trong Codex tạo ngày 28/11/2026. Một tranh nền dùng cho cả hai cảnh. Trăng, đường nứt, bốn hình mảnh khớp nhau, hạt hình thoi, chuyển động rễ, bè và các cơ cấu cầu, cổng được dựng riêng trong Unity. Sprite Cuội, Cây Đa, lồng đèn, thân bè và thùng lấy từ thư viện đã ghi công của project. [Phông đêm](../Assets/Moonlit/Resources/FourLandsAtNight.png) · [Nguồn Cây Đa](BANYAN_DUEL.md) · [Nguồn mỹ thuật chung](../CREDITS.md)

Các lần đổi nhịp giữa nhảy bệ, đi bè và giải đố có tham khảo cách bố trí chướng ngại của Donkey Kong Country Returns. Cổng trúc, bàn cân và cầu gỗ là thiết kế riêng cho bốn màn. [Nintendo, Donkey Kong Country Returns HD](https://www.nintendo.com/us/store/products/donkey-kong-country-returns-hd-switch/)

**Prompt phông đêm**

> Panoramic 16:9 full-bleed handcrafted 16-bit pixel art environment plate for the opening and ending of a Vietnamese Mid-Autumn folk tale game. The entire top 72 percent is a spacious deep midnight-indigo sky, sparse tiny stars, two thin azure cloud wisps on the far edges. Leave the center of the sky clear for an animated moon and banyan sprite supplied by the game. The bottom quarter is an intricately tiered silhouette of northern Vietnam seen from a high hill: on the LEFT a bamboo-fenced hamlet with a few amber star lanterns and tiled roofs, mid-left a dense blue-green bamboo forest, mid-right a curving river dotted with paper lights, on the RIGHT a distant hill with a broad dark banyan. Delicate reflected amber pixels, atmospheric cobalt ridgelines. Quiet, magical, exquisite restrained pixel clusters, richly composed in a wide 16:9 frame like a finished scrolling storybook videogame background. NO MOON, no glowing disk, no giant tree in sky, no characters, no letters, no labels, no grids or borders. Opaque background.

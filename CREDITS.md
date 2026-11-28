# Cuội Tìm Trăng · Bản quyền và nguồn tư liệu

Bản Windows 2.7.0, ngày 28/11/2026. Câu chuyện dựa trên tích dân gian Chú Cuội và cây đa. Tên trò chơi, bốn địa danh, phần thưởng và hướng kết truyện theo tài liệu thiết kế “Cuội Tìm Trăng (Màn 1)” do nhóm cung cấp.

## Mỹ thuật

| Thành phần trong game | Nguồn và cách tạo |
|---|---|
| Cuội, chó, mèo, môi trường rừng, đá, cành và mảnh trăng | Các atlas và sprite do OpenAI ImageGen tạo cho project gốc. [Danh mục và cách cắt atlas](Documentation/ASSETS.md) · [Prompt ban đầu](Documentation/asset-prompts.json) |
| Cuội đứng ngắm đèn, cúi người, chạy, leo vách và trèo qua mép | Hoạt ảnh đứng dùng các dáng Cuội trong atlas gốc. OpenAI ImageGen tạo các bộ sprite cúi, chạy và leo dựa trên hình Cuội của trò chơi. Mỗi dáng cúi có điểm đặt bàn chân và điểm cầm đèn riêng. [Nguồn hình và mô tả tạo sprite](Documentation/CUOI_ANIMATION_CREDITS.md) |
| Cá chép đồng và quạ | Hai atlas hoạt hình gốc, mỗi loài có bốn tư thế. Bọt nước, quỹ đạo nhảy và sà, vệt báo trước và đốm sáng khi choáng được dựng trong Unity. [Atlas, prompt và nguồn năm hiệu ứng âm thanh](Documentation/WETLAND_CREATURE_CREDITS.md) |
| Bè, thùng, gỗ, lá và dây leo bổ sung | Atlas ForestExtras tạo bằng ImageGen cho project. [Prompt](Documentation/polish-asset-prompt.txt) |
| Bộ ảnh bệ rêu và hạt trăng lưu trong project | Ảnh ImageGen ngày 19–20/11. Trong bản 2.0, hạt trăng hình thoi được vẽ theo điểm ảnh bằng MoonlitVisuals.cs. [Prompt và ảnh tham chiếu của mẫu rêu](Documentation/readability-asset-prompts.md) |
| Bốn phông toàn cảnh, từ làng đến đồi đa | Bốn ảnh gốc tạo bằng OpenAI ImageGen tích hợp ngày 24/11. [Tên file và nguyên văn từng prompt](Documentation/campaign-scenery-sources.json) |
| Bờ xa, bờ gần và tiền cảnh của cả bốn địa danh | Bốn atlas Embankment và atlas EdgeHabitat do OpenAI ImageGen vẽ theo prompt của project, gồm cả đất, mặt nước, kè và móng công trình cùng lớp. [Tên file, prompt và cách tách lớp](Documentation/landscape-sources.json) |
| Mặt đường và mặt cắt địa hình | Bốn vật liệu gốc VillageEarth, BambooLoam, RiverStone, BanyanSlate do OpenAI ImageGen tạo. Shader của project lấy mẫu sắc độ ở các pixel liền kề để điều chỉnh ánh đèn trên bề mặt đá, đất và rễ. [Prompt từng vật liệu](Documentation/terrain-sources.json) |
| Mép đi lại trên bờ và vách | Bốn dải màu alpha riêng cho làng, rừng trúc, bến sông và đồi đa được tạo bằng OpenAI ImageGen. Mesh và shader của project biến đổi đường viền, mối nối và mặt đá lộ ra theo vị trí trong màn. [Prompt, cách đóng atlas và mã kiểm ảnh](Documentation/surface-crests-source.json) |
| Cỏ bờ nước, sen và lá nổi | Sáu cụm thực vật vẽ riêng bằng OpenAI ImageGen. Mặt nước, những vệt phản chiếu của bờ và ánh đèn được dựng bằng shader của project. [Prompt, tọa độ cắt và mã kiểm ảnh](Documentation/wetland-sources.json) |
| Cổng tre, nhà tranh, thuyền có mui và cây đa nhỏ | Bốn sprite cắt từ một atlas alpha gốc do OpenAI ImageGen tạo ngày 24/11. [Prompt, tọa độ cắt và mã kiểm ảnh](Documentation/campaign-landmark-sources.json) |
| Cây Đa, cánh rễ bện và đèn ông sao | Cây Đa với mặt khắc pixel cùng cánh rễ buộc vải điều được vẽ riêng bằng OpenAI ImageGen. [Prompt, nguồn tham khảo trận đấu và tiếng động](Documentation/BANYAN_DUEL.md) · [Đèn ông sao](Documentation/GUARDIAN_AUDIO_CREDITS.md) |
| Hoạt cảnh trăng vỡ, trăng liền, hai bàn đạp và chuông đồng trên giàn tre, cổng trúc, cầu bản lề, chông, bàn cân và bè cống | Phông bốn miền được tạo bằng OpenAI ImageGen. Đĩa trăng và vật thể tương tác được dựng trong Unity trên thư viện của project. [Nguồn, prompt và ý tưởng thiết kế](Documentation/MOON_CROSSINGS.md) |
| Tranh trang chủ và tranh đoàn tụ trên Cung Trăng | Hai tranh gốc tạo bằng OpenAI ImageGen ngày 24/11. [Tên file và prompt](Documentation/campaign-ui-sources.json) |
| Tim đầy và tim đã mất | Hai sprite 15 × 12 pixel vẽ riêng cho game. Tim đầy dùng màu san hô và đốm sáng. Tim đã mất có viền xám sáng và lòng tối. [Tên ảnh và mã kiểm SHA-256](Documentation/campaign-ui-sources.json) |
| Lớp cảnh cuộn, mặt nước, bụi, đom đóm, chớp đèn, rễ quét, mầm nở và biểu tượng giao diện | Dựng trong Unity bằng sprite của project, hình học, shader và chuyển động theo mã. Clip thành phố `chrome_VSn4muprhL.mp4` do người dùng cung cấp là tài liệu tham chiếu chuyển động. |

Bản đồ vùng đã đi và mép sương được vẽ trong Unity từ địa hình của từng màn. Cách giữ bí mật những hạt trăng ngoài tầm khám phá có tham khảo hệ thống bản đồ của Dead Cells. [Nhật ký cập nhật của Motion Twin và Evil Empire](https://store.steampowered.com/news/posts/?appgroupname=Dead+Cells&appids=588650&enddate=1573130020)

## Âm thanh

Sáu bản nhạc nền và hai câu kết dùng giai điệu dân gian Trống quân của Bắc Bộ và Đi cấy trong tổ khúc múa đèn Đông Anh, Thanh Hóa. Game có hòa âm và diễn tấu piano riêng với tiếng đàn Salamander Grand Piano của Alexander Holm, giấy phép CC BY 3.0. Tiếng động của nhân vật, động vật, lồng đèn và giao diện được tổng hợp riêng. Bản 2.3 bổ sung hai nền âm thanh nổi (lá trúc và nước sông) cùng tiếng chốt gỗ gài và mặt cầu chạm mố. [Mã tổng hợp và tham số](Tools/ScoreMoonlit.py). [Bản ký âm, nguồn tiếng đàn và cách phối từng cảnh](Documentation/FESTIVAL_PIANO.md) · [Danh mục tiếng động](Documentation/GUARDIAN_AUDIO_CREDITS.md)

## Chữ và công nghệ

- Liberation Sans đi kèm TextMesh Pro Essential Resources. Bản quyền dữ liệu phông thuộc Google Corporation (2010) và Red Hat, Inc. (2012). Đính kèm giấy phép SIL Open Font License 1.1. [Toàn văn giấy phép](<Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt>)
- Các tiêu đề dùng Constantia khi phông này có sẵn trên Windows. Game dùng Liberation Sans làm phông dự phòng.
- Unity 6000.3.16f1, Universal Render Pipeline 2D, Input System, uGUI và TextMesh Pro của Unity Technologies. [Thông báo giấy phép Unity UI](Documentation/Unity-UI-LICENSE.md)


## Tham khảo chỉ đạo mỹ thuật

Nhóm tham khảo cách các họa sĩ Motion Twin mô tả độ nổi của hình khối, màu sắc và phản ứng với nguồn sáng trong Dead Cells. Nguồn hình ảnh và âm thanh sử dụng trong Cuội Tìm Trăng được ghi theo từng nhóm ở trên. [Bài viết của Gwenaël Massé](https://www.gamedeveloper.com/production/art-design-deep-dive-giving-back-colors-to-cryptic-worlds-in-i-dead-cells-i-) · [Bài viết của Thomas Vasseur](https://www.gamedeveloper.com/production/art-design-deep-dive-using-a-3d-pipeline-for-2d-animation-in-i-dead-cells-i-)

Khi bố trí cá và quạ, nhóm tham khảo cách Takashi Tezuka và Shigeru Miyamoto dự đoán hướng di chuyển của người chơi và đặt địa hình quanh điểm gặp kẻ địch. [Phỏng vấn của Nintendo về thiết kế màn Super Mario Bros.](https://www.nintendo.com/en-gb/Iwata-Asks/Super-Mario-Bros-25th-Anniversary/Vol-5-Original-Super-Mario-Developers/4-Designing-Levels-Together/4-Designing-Levels-Together-212908.html)

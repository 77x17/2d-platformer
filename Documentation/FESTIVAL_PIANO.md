# Nhạc piano trong Cuội Tìm Trăng

Sáu bản nền và hai đoạn kết được phối, diễn tấu và xuất riêng cho game ngày 25/11/2026. Phần giai điệu có nguồn từ hai làn điệu dân gian Việt Nam. Tiếng đàn lấy từ các nốt piano thu âm của Alexander Holm.

## Hai giai điệu

**Trống quân, dân ca Bắc Bộ.** Bản phối dùng câu mở và câu lưu không của dị bản có lời mở đầu “Ở đây đất thấp trời cao”, trong *Dân ca Việt Nam – Những làn điệu dân ca nổi tiếng phổ biến khắp nơi*, do Quang Phác và Đào Ngọc Dung sưu tầm, tuyển chọn và giới thiệu, Nhà xuất bản Hà Nội, 2001, trang in 85–86, tương ứng trang PDF 83–84. Nhóm chép lại các cao độ và trường độ của phần đơn âm rồi viết phần đệm, bè dưới và lần nhắc. Cục Di sản văn hóa ghi nhận hát trống quân là sinh hoạt hát đối đáp trong dịp Tết Trung thu ở Bắc Bộ. [Bản ký âm, trang PDF 83](https://medialib.qlgd.edu.vn/Uploads/THU_VIEN/shn/1/1658/UserFiles/Dan-Ca-Viet-Nam---Nhung-Lan-Dieu-Dan-Ca-Noi-Tieng-Pho-Bien-Khap-Noi-f11a0530-4d99-47bf-8096-eafbe8d90580.pdf#page=83) · [Cục Di sản văn hóa](https://dsvh.gov.vn/hat-trong-quan-1301)

**Đi cấy, tổ khúc múa đèn Đông Anh, Thanh Hóa.** Nhóm chép khuông đầu và nốt hạ câu “thắp đèn … chơi trăng ngoài thềm” ở ví dụ 6, trang in 42 trong bài nghiên cứu của Vi Minh Huy năm 2021. Đây là ví dụ ký âm làn điệu dân gian trong múa đèn. Ví dụ 7 bên dưới là ca khúc của Phạm Tịnh. Trong game, sau nét luyến bộ ba và câu hạ về Sol của làn điệu dân gian, đàn trả lời bằng một câu mới. [Vi Minh Huy, *The Melody of Thanh Hoa Folk Songs in the Songs on Thanh Land*, ví dụ 6](https://tckh.dvtdt.edu.vn/index.php/tckh/article/download/45/89/108#page=6)

Hai nguồn ghi các làn điệu là dân ca, không ghi tác giả giai điệu. Bản phát hành dùng phần giai điệu truyền thống với hòa âm, tốc độ, phân câu, nhấn phím và bố cục do project thực hiện. Các trang sách, ca từ, bản hòa âm của nghệ sĩ khác và bản thu một bài hát hoàn chỉnh đều nằm ngoài bộ asset phân phối.

## Các bản phối

| Tệp Vorbis | Thời lượng | Nguồn và cách diễn tấu |
|---|---:|---|
| mus_home | 38,92 s | **Đi cấy**, 74 BPM. Đàn thưa, giữ pedal qua phách. Nửa sau chuyển câu đáp lên một quãng tám. |
| mus_village | 32,00 s | **Trống quân**, 90 BPM. Bè bass và hợp âm ngắn đối đáp, giai điệu chuyển lên một cung. |
| mus_bamboo | 27,43 s | **Trống quân**, 105 BPM. Nâng lên một quãng bốn đúng, bè trái gọn và nhấn vào phách yếu. |
| mus_river | 41,14 s | **Đi cấy**, 70 BPM. Hợp âm rải từ thấp lên cao, pedal dài. Câu đáp lần hai nằm ở âm khu cao. |
| mus_hill | 36,00 s | **Đi cấy**, 80 BPM. Hạ một cung, quãng năm trầm rải chậm. Ở lần nhắc, những nốt dài có thêm bè quãng tám phía dưới. |
| mus_guardian | 25,71 s | **Trống quân**, 112 BPM. Bass nhắc ngắn, câu chính rõ ở tay phải. Những nốt nhấn cuối đoạn dày thêm một quãng tám. |
| jingle_win | 4,30 s | Nét mở phỏng theo **Đi cấy**, tiếp bằng câu kết riêng lên Sol cao. |
| jingle_lose | 3,40 s | Câu kết viết riêng từ cụm Sol–Rê–Đô–Rê của **Trống quân**, giảm lực tay và hạ âm vực. |

## Nguồn tiếng đàn và giấy phép

**Salamander Grand Piano** của **Alexander Holm** được phát hành theo **Creative Commons Attribution 3.0 Unported**. Nhóm dùng bản MP3 từng nốt do Tone.js phân phối. Quá trình dựng gồm cắt im lặng đầu nốt, chuyển cao độ giữa các phím lân cận, thay đổi lực và thời gian nhấn, tạo tiếng nhả phím bằng bao âm, đặt vị trí stereo và thêm phản xạ phòng. Các bản nhạc đã kết xuất chứa những tiếng đàn được sử dụng theo giấy phép này. [Tác giả và giấy phép trong bộ mẫu](https://github.com/Tonejs/audio/blob/master/salamander/README) · [Giấy phép CC BY 3.0](https://creativecommons.org/licenses/by/3.0/) · [Các mẫu MP3 dùng để dựng](https://github.com/Tonejs/audio/tree/master/salamander)

## Dựng lại

1. Lấy 17 file MP3 trong thư mục nguồn ở trên. Với mỗi quãng tám từ 2 đến 5, lấy C, Ds, Fs và A, ví dụ `C2.mp3`, `Ds2.mp3`, `Fs2.mp3`, `A2.mp3`. Lấy thêm `C6.mp3`.
2. Cài Python 3 với NumPy, FFmpeg và libsndfile có hỗ trợ Ogg/Vorbis. FFmpeg giải mã các mẫu MP3. libsndfile xuất stereo 32 kHz và giữ độ dài theo số mẫu cho các đoạn lặp.
3. Từ gốc project, chạy lệnh dưới đây. Trên Windows, có thể thêm `--ffmpeg "C:/.../ffmpeg.exe" --sndfile "C:/.../sndfile.dll"` nếu các thư viện nằm ngoài đường dẫn mặc định.

```sh
python Tools/FestivalPiano.py --samples path/to/salamander
```

Trình dựng định nghĩa riêng các nốt trích từ bản ký âm và phần đáp viết cho game trong hai bảng `TRONG_QUAN` và `DI_CAY`. Sáu bản nền vòng lại bằng cách đưa phần ngân cuối nốt sang đầu vòng. Các tệp được ghi vào `Assets/Moonlit/Resources/MoonAudio`. [Mã dựng các bản piano](../Tools/FestivalPiano.py)

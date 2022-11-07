video_audio.mp4
--------------------------------------------------------------------------------

Command:

ffmpeg -f lavfi -i testsrc=duration=5:size=640x360:rate=30000/1001:decimals=2 -f lavfi -i sine=220:4:duration=5 -c:a aac -profile:v baseline -pix_fmt yuv420p -color_primaries bt709 -color_trc bt709 -colorspace bt709 -color_range pc -metadata creation_time="2022-11-06T12:34:56" video_audio.mp4
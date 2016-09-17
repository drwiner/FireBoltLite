@ECHO OFF
cd FireBolt
FireBolt.exe
cd ../ffmpeg/bin
ffmpeg -framerate 5 -i ..\..\FireBolt\.screens\img%01d.png -c:v libx264 -r 30 -pix_fmt yuv420p out.mp4
ffmpeg -y -i out.mp4 -vcodec copy ..\..\out.avi
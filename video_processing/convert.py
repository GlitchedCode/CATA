import sys
import os

from moviepy.editor import VideoFileClip, vfx, ColorClip

white = ColorClip(color=(255, 255, 255), size=(1, 1))

video = VideoFileClip(sys.argv[1])
video = video.subclip(2, -2).set_fps(10)
video = video.fx(vfx.lum_contrast, 1, 20).fx(vfx.blackwhite)
video = video.fx(vfx.mask_color, (255, 255, 255), 0.5)
video = video.resize(height=200)

os.makedirs("out", exist_ok=True)
video.write_videofile("out/out.ogv")

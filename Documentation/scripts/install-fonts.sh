#!/bin/bash

FONTS="\
apache/robotomono/RobotoMono-Bold.ttf \
apache/robotomono/RobotoMono-BoldItalic.ttf \
apache/robotomono/RobotoMono-Italic.ttf \
apache/robotomono/RobotoMono-Light.ttf \
apache/robotomono/RobotoMono-LightItalic.ttf \
apache/robotomono/RobotoMono-Medium.ttf \
apache/robotomono/RobotoMono-MediumItalic.ttf \
apache/robotomono/RobotoMono-Regular.ttf \
apache/robotomono/RobotoMono-Thin.ttf \
apache/robotomono/RobotoMono-ThinItalic.ttf \
ofl/ptmono/PTM55FT.ttf \
ofl/ptsans/PT_Sans-Web-Bold.ttf \
ofl/ptsans/PT_Sans-Web-BoldItalic.ttf \
ofl/ptsans/PT_Sans-Web-Italic.ttf \
ofl/ptsans/PT_Sans-Web-Regular.ttf \
ofl/ptsanscaption/PT_Sans-Caption-Web-Bold.ttf \
ofl/ptsanscaption/PT_Sans-Caption-Web-Regular.ttf \
ofl/ptsansnarrow/PT_Sans-Narrow-Web-Bold.ttf \
ofl/ptsansnarrow/PT_Sans-Narrow-Web-Regular.ttf \
ofl/ptserif/PT_Serif-Web-Bold.ttf \
ofl/ptserif/PT_Serif-Web-BoldItalic.ttf \
ofl/ptserif/PT_Serif-Web-Italic.ttf \
ofl/ptserif/PT_Serif-Web-Regular.ttf \
ofl/ptserifcaption/PT_Serif-Caption-Web-Italic.ttf \
ofl/ptserifcaption/PT_Serif-Caption-Web-Regular.ttf \
"

LOCAL_FONTS="\
roadradio \
"

FONTDIR=~/.local/share/fonts

set -e
cd "$(dirname "$(readlink -f "${0}")")"

mkdir -p $FONTDIR

for FONT in $FONTS
do
  IFS='/' read -ra PARTS <<< "$FONT"
  FILENAME=${PARTS[-1]}
  FONTFAMILY=${PARTS[-2]}
  if [ ! -f $FONTDIR/$FONTFAMILY/$FILENAME ]; then
    mkdir -p $FONTDIR/$FONTFAMILY
    wget "https://github.com/google/fonts/tree/master/$FONT"
    mv -v $FILENAME $FONTDIR/$FONTFAMILY/
  fi
done

for LOCAL_FONT in $LOCAL_FONTS
do
  if [ ! -d $FONTDIR/$LOCAL_FONT ]; then
    cp -vrf $LOCAL_FONT $FONTDIR/
  fi
done

fc-cache -f -v

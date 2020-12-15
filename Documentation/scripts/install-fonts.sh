#!/bin/bash

FONTS="\
apache/roboto/static/Roboto-Black.ttf \
apache/roboto/static/Roboto-BlackItalic.ttf \
apache/roboto/static/Roboto-Bold.ttf \
apache/roboto/static/Roboto-BoldItalic.ttf \
apache/roboto/static/Roboto-Italic.ttf \
apache/roboto/static/Roboto-Light.ttf \
apache/roboto/static/Roboto-LightItalic.ttf \
apache/roboto/static/Roboto-Medium.ttf \
apache/roboto/static/Roboto-MediumItalic.ttf \
apache/roboto/static/Roboto-Regular.ttf \
apache/roboto/static/Roboto-Thin.ttf \
apache/roboto/static/Roboto-ThinItalic.ttf \
apache/roboto/static/RobotoCondensed-Bold.ttf \
apache/roboto/static/RobotoCondensed-BoldItalic.ttf \
apache/roboto/static/RobotoCondensed-Italic.ttf \
apache/roboto/static/RobotoCondensed-Light.ttf \
apache/roboto/static/RobotoCondensed-LightItalic.ttf \
apache/roboto/static/RobotoCondensed-Regular.ttf \
apache/robotomono/static/RobotoMono-Bold.ttf \
apache/robotomono/static/RobotoMono-BoldItalic.ttf \
apache/robotomono/static/RobotoMono-Italic.ttf \
apache/robotomono/static/RobotoMono-Light.ttf \
apache/robotomono/static/RobotoMono-LightItalic.ttf \
apache/robotomono/static/RobotoMono-Medium.ttf \
apache/robotomono/static/RobotoMono-MediumItalic.ttf \
apache/robotomono/static/RobotoMono-Regular.ttf \
apache/robotomono/static/RobotoMono-Thin.ttf \
apache/robotomono/static/RobotoMono-ThinItalic.ttf \
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

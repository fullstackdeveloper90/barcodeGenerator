FROM mono:onbuild

EXPOSE 1234 8888

CMD ["mono", "./BarcodeService.exe"] 
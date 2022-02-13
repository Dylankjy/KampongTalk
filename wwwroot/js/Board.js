function expandImg(img) {
    var imgSrc = img.src
    document.getElementById("ImageModal").style.display = "block"
    document.getElementById("imgInModal").src = imgSrc
}


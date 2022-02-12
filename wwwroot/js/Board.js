function likePost(postSpan, likeBtn) {
    var Pid = postSpan.innerText
    console.log("frontend: " + Pid)
    $.ajax({
        type: "POST",
        url: "/Board/like",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: {
            EntityId: Pid
        },
        success: function (likeResp) {
            var likeRespArr = likeResp.toString().split(",")
            if (likeRespArr[0] == '1') {
                // likeBtn.firstElementChild.classList.remove('is-light')
                likeBtn.firstElementChild.innerHTML = '<i class="fas fa-thumbs-up"></i>&ensp; Like';
            }
            else {
                //likeBtn.firstElementChild.classList.add('is-light')
                likeBtn.firstElementChild.innerHTML = '<i class="far fa-thumbs-up"></i>&ensp; Like';
            }
            likeBtn.lastElementChild.innerHTML = likeRespArr[1]

        },
        error: function () {
            return "error";
        }

    })
}

function expandImg(img) {
    var imgSrc = img.src
    document.getElementById("ImageModal").style.display = "block"
    document.getElementById("imgInModal").src = imgSrc
}




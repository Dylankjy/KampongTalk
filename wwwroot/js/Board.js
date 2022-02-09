var dropdownList = document.getElementsByClassName("dropdown");
    for (const d of dropdownList) {
        d.addEventListener('click', function (event) {
            event.stopPropagation();
            d.classList.toggle('is-active');
        });
}

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


    var translateText = (obj) => {
        var translateString = $(obj).prev().text()
        // Use the TranslateAPIController.cs, and run the Translate() function
        var url = '@Url.Action("Translate", "TranslateAPI")'
        console.log(translateString)
// Pass in the parameters as {"text": blahblah, "language": blahblah}
        $.get(url, {text: translateString, language: "@UserAttributes.getTranslateLanguage()" })
            .done((data) => {
            $(obj).prev().text(data)
        })
    }

        // Keep a list of all the generated filenames, so we can delete them once the user does not need thems
        var filenames = []

    var deleteFile = (filename) => {
        // "Delete" refers to the function name, not the route name as specified in [Route] within your API controller
        var deleteUrl = '@Url.Action("Delete", "SpeechAPI")'
        console.log(deleteUrl)
        $.get(deleteUrl, {filename: filename })
        .done((data) => {
            console.log(data)
        })
    }

    var synthesizeSpeech = (obj) => {
        // Use the TranslateAPIController.cs, and run the Translate() function
        var url = '@Url.Action("AutoSynthesize", "SpeechAPI")'
        var ttsText = $(obj).prev().prev().text()

        // Pass in the parameters as {"text": blahblah, "language": blahblah}
        $.get(url, {text: ttsText, gender: "@UserAttributes.getSpeechGender()" })
            .done((data) => {
                var filename = `speech/${data}`
        filenames.push(filename)
        $(obj).next().attr("src", filename)
        $(obj).next()[0].play()
            })

        $(window).bind('beforeunload', () => {
            filenames.forEach((filename) => {
                deleteFile(filename)
            })
        })
    }

    // Delete all the synthesized files when the user navigates to another page
    // console.log("This has ran")
    $(window).bind('beforeunload', () => {
            filenames.forEach((filename) => {
                deleteFile(filename)
            })
        })

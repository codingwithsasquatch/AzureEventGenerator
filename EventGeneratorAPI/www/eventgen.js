/*eslint eqeqeq: ["error", "smart"]*/

function startButtonClick() {
    var messageDuration = document.getElementById('messageDuration').value;
    var messageFrequency = document.getElementById('messageFrequency').value;
    var messageMethod = document.getElementById('messageService').value;
    var messageScheme = document.querySelector('input[name="messageScenario"]:checked').value;

    var jobRequest = {
        "messageMethod": messageMethod,
        "duration": messageDuration,
        "frequency": messageFrequency,
        "messageScheme": messageScheme
    }

    switch (messageMethod) {
        case "eventgrid":
            jobRequest.endpoint = document.getElementById('egEndpoint').value;
            jobRequest.key = document.getElementById('egKey').value;
            break;
        case "eventhub":
            jobRequest.connectionString = document.getElementById('ehConnectionString').value;
            jobRequest.eventhub = document.getElementById('ehEventHub').value;
            break;
        case "servicebus":
            jobRequest.connectionString = document.getElementById('sbConnectionString').value;
            jobRequest.queue = document.getElementById('sbQueue').value;
            break;
        case "storagequeue":
            jobRequest.connectionString = document.getElementById('sqConnectionString').value;
            jobRequest.queue = document.getElementById('sqQueue').value;
            break;    
    }

    console.log(jobRequest);
    var xhttp = new XMLHttpRequest();
    xhttp.open("POST", "/job", true);
    xhttp.setRequestHeader("Content-type", "application/json");
    xhttp.send(JSON.stringify(jobRequest));
    console.log(xhttp.responseText)


    document.getElementById('stopButton').disabled = false;
    document.getElementById('startButton').disabled = true;
    setTimeout(function () {
        document.getElementById('startButton').disabled = false;
        document.getElementById('stopButton').disabled = true;
    }, 60*1000*messageDuration)
}

function stopButtonClick() {
    var xhttp = new XMLHttpRequest();
    xhttp.open("DELETE", "/job/", true);
    xhttp.setRequestHeader("Content-type", "application/json");
    xhttp.send();
    console.log(xhttp.responseText)

    document.getElementById('startButton').disabled = false;
    document.getElementById('stopButton').disabled = true;
}

function messageScenarioClicked() {
    switch (document.querySelector('input[name="messageScenario"]:checked').value)
     {
        case 'ninjabattle':
            document.getElementById('messageScenarioDescription').innerHTML = "Epic battle between good and bad ninjas played out through sensors on the battlefield";
            break;
        case 'simple':
            document.getElementById('messageScenarioDescription').innerHTML = "Simple enumerated messages";
            break;
    }
}

function messageServiceChanged() {
    messageServiceSettingsChanged();
    Array.prototype.slice.call(document.querySelectorAll('div[id$="MessageServiceSettings"]')).forEach( function(el) {
        el.hidden = true;
    });
    switch (document.getElementById('messageService').value) {
        case "eventgrid":
            document.getElementById('egMessageServiceSettings').hidden = false;
            break;
        case "eventhub":
            document.getElementById('ehMessageServiceSettings').hidden = false;
            break;
        case "servicebus":
            document.getElementById('sbMessageServiceSettings').hidden = false;
            break;
        case "storagequeue":
            document.getElementById('sqMessageServiceSettings').hidden = false;
            break;    
    }
}

function messageServiceSettingsChanged() {
    switch (document.getElementById('messageService').value) {
        case "eventgrid":
            var hasBlanks = [];
            // get egSettings
            Array.prototype.slice.call(document.querySelectorAll('*[class$="egMessageServiceSetting"]')).forEach(function(el) {el.value == '' ? hasBlanks.push(true): hasBlanks.push(false);});
            if (hasBlanks.indexOf(true) == -1) {
                console.log("no more blanks");
                document.getElementById('MessageSendSettings').hidden = false;
            } else {
                console.log("blanks still");
                document.getElementById('MessageSendSettings').hidden = true;
            }
            break;
        case "eventhub":
            var hasBlanks = [];
            // get ehSettings
            Array.prototype.slice.call(document.querySelectorAll('*[class$="ehMessageServiceSetting"]')).forEach(function(el) {el.value == '' ? hasBlanks.push(true): hasBlanks.push(false);});
            if (hasBlanks.indexOf(true) == -1) {
                console.log("no more blanks");
                document.getElementById('MessageSendSettings').hidden = false;
            } else {
                console.log("blanks still");
                document.getElementById('MessageSendSettings').hidden = true;
            }
            break;
        case "servicebus":
            var hasBlanks = [];
            // get sbSettings
            Array.prototype.slice.call(document.querySelectorAll('*[class$="sbMessageServiceSetting"]')).forEach(function(el) {el.value == '' ? hasBlanks.push(true): hasBlanks.push(false);});
            if (hasBlanks.indexOf(true) == -1) {
                console.log("no more blanks");
                document.getElementById('MessageSendSettings').hidden = false;
            } else {
                console.log("blanks still");
                document.getElementById('MessageSendSettings').hidden = true;
            }
            break;
        case "storagequeue":
            var hasBlanks = [];
            // get sqSettings
            Array.prototype.slice.call(document.querySelectorAll('*[class$="sqMessageServiceSetting"]')).forEach(function(el) {el.value == '' ? hasBlanks.push(true): hasBlanks.push(false);});
            if (hasBlanks.indexOf(true) == -1) {
                console.log("no more blanks");
                document.getElementById('MessageSendSettings').hidden = false;
            } else {
                console.log("blanks still");
                document.getElementById('MessageSendSettings').hidden = true;
            }
            break; 
    }
}

Array.prototype.slice.call(document.querySelectorAll('input[name="messageScenario"]')).forEach(function(el) {el.addEventListener('click', messageScenarioClicked);});
document.getElementById('messageService').addEventListener('change', messageServiceChanged);
Array.prototype.slice.call(document.querySelectorAll('*[class$="MessageServiceSetting"]')).forEach(function(el) {el.addEventListener('input', messageServiceSettingsChanged);});
document.getElementById('startButton').addEventListener('click', startButtonClick);
document.getElementById('stopButton').addEventListener('click', stopButtonClick);
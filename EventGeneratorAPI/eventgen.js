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
    xhttp.open("POST", "http://localhost:7071/api/job", true);
    xhttp.setRequestHeader("Content-type", "application/json");
    xhttp.send(JSON.stringify(jobRequest));
    console.log(xhttp.responseText)


    document.getElementById('stopButton').disabled = false;
    document.getElementById('startButton').disabled = true;
    setTimeout(() => {
        document.getElementById('startButton').disabled = false;
        document.getElementById('stopButton').disabled = true;
    }, 60*1000*messageDuration)
}

function stopButtonClick() {
    var xhttp = new XMLHttpRequest();
    xhttp.open("DELETE", "http://localhost:7071/api/job/", true);
    xhttp.setRequestHeader("Content-type", "application/json");
    xhttp.send(jobRequest);
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
    document.querySelectorAll('div[id$="MessageServiceSettings"]').forEach( function(el) {
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
            document.querySelectorAll('*[class$="egMessageServiceSetting"]').forEach((el) => {el.value == '' ? hasBlanks.push(true): hasBlanks.push(false);});
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
            document.querySelectorAll('*[class$="ehMessageServiceSetting"]').forEach((el) => {el.value == '' ? hasBlanks.push(true): hasBlanks.push(false);});
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
            document.querySelectorAll('*[class$="sbMessageServiceSetting"]').forEach((el) => {el.value == '' ? hasBlanks.push(true): hasBlanks.push(false);});
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
            document.querySelectorAll('*[class$="sqMessageServiceSetting"]').forEach((el) => {el.value == '' ? hasBlanks.push(true): hasBlanks.push(false);});
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

document.querySelectorAll('input[name="messageScenario"]').forEach((el)=> {el.addEventListener('click', messageScenarioClicked);});
document.getElementById('messageService').addEventListener('change', messageServiceChanged);
document.querySelectorAll('*[class$="MessageServiceSetting"]').forEach((el) => {el.addEventListener('input', messageServiceSettingsChanged);});
document.getElementById('startButton').addEventListener('click', startButtonClick);
document.getElementById('stopButton').addEventListener('click', stopButtonClick);
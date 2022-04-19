const openModalButtons = document.querySelectorAll('[data-modal-target]');
const dropdownButtons = document.querySelectorAll('data-dropdown-modifier');
const joinGameButton = document.querySelector('[data-join-game-button]');
const createGameButton = document.querySelector('[data-create-game-button]');
const closeModalButtons = document.querySelectorAll('[data-close-button]');
const overlay = document.getElementById('modal-overlay');



openModalButtons.forEach( button => {
    button.addEventListener('click', () =>{
        const modal = document.querySelector(button.dataset.modalTarget);
        openModal(modal);
    });
})

closeModalButtons.forEach( button => {
    button.addEventListener('click', () =>{
        const modal = button.closest('.modal');
        closeModal(modal);
    });
})

dropdownButtons.forEach(button =>{
    button.addEventListener('click', () =>{
        const dropdown = button.closest('.dropdown');
        dropdown.value = button.innerHTML;
    });
})

joinGameButton.addEventListener('click', () =>{
    //Stop more than one chunk of data from being sent
    const galaxy = document.getElementById('join-game-world-name');
    const username = document.getElementById('join-game-player-name');
    const password = document.getElementById('join-game-player-password');
    if(galaxy.innerHTML.length >=3 && galaxy.innerHTML.length <=20 && username.innerHTML.length >=3 && username.innerHTML.length <=20 && password.innerHTML.length >=3 && password.innerHTML.length <=20){
        let ourRequest = new XMLHttpRequest();
        ourRequest.onerror = function(){
            alert("HTTP ERROR. Unable to join game");
        }
        ourRequest.onLoad = function() {
            if(ourRequest.status==200){
                sessionStorage.setItem("JWT",ourRequest.responseText);
                window.location.href = "spaceDiplomacyGameStarfield";
            }else{
                console.log("HTML GET Join game was unable to find data");
            }
        }
        document.body.style.cursor = "progress";

        ourRequest.open('POST', 'logins',true);
        ourRequest.send(JSON.stringify({galaxy:galaxy,username:username,password:password}));
        console.log("Send request: " + galaxy.value + "from " + username.value + " pwd " + password.value);
    }else{
        alert("Your username, password, and world must be between 3 and 20 characters");
    }
})

createGameButton.addEventListener('click', () =>{
    //Stop more than one chunk of data from being sent
    const count = document.getElementById('create-game-player-count');
    const time = document.getElementById('create-game-time-between-turns');
    const username = document.getElementById('create-game-player-name');
    const password = document.getElementById('create-game-player-password');
    if(count >=2 && count <=8 && time.innerHTML.length >=3 && time.innerHTML.length <=20 && username.innerHTML.length >=3 && username.innerHTML.length <=20 && password.innerHTML.length >=3 && password.innerHTML.length <=20){
        let ourRequest = new XMLHttpRequest();
        ourRequest.onerror = function(){
            alert("HTTP ERROR. Unable to create game");
        }
        ourRequest.onLoad = function() {
            if(ourRequest.status==200){
                window.location.href = "spaceDiplomacyGameStarfield";
            }
            if(ourRequest.status==404){
                alert("Galaxy does not exist or is full");
            }
        }

        ourRequest.open('POST', 'logins',true);   
        ourRequest.send(JSON.stringify({username:username,password:password,count:count.value,time:time.value}));
        console.log("Send request: " + galaxy.value + "from " + username.value + " pwd " + password.value);
    }else{
        //TODO Display username/password error to player better
        alert("Your username, password, and world must be between 3 and 20 characters");
    }
})


/*
let ourRequest = new XMLHttpRequest();
ourRequest.onerror = function(){
    //TODO handle error
}
ourRequest.open('GET', 'TODO URL');
ourRequest.setRequestHeader('Authorization', 'Bearer' + TODO JWT TOKEN)
ourRequest.onLoad = function() {
    JSON.parse(ourRequest.responseText)
    //TODO populate world and factions
}
ourRequest.send()
*/

function openModal(modal){
    if (modal == null) return;
    modal.classList.add('active');
    overlay.classList.add('active');
}

function closeModal(modal){
    if (modal == null) return;
    modal.classList.remove('active');
    overlay.classList.remove('active');
}
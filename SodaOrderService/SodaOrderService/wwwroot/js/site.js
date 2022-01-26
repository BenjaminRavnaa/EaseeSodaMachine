const uri = 'api/sodaorders';
let sodaOrders = [];

function getItems() {
    fetch(uri)
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => console.error('Unable to get items.', error));
}

function addOrder() {
    const addOrderSelector = document.getElementById('soda-selector');

    const item = {
        isComplete: false,
        soda: addOrderSelector.value
    };

    fetch(uri, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    })
        .then(response => response.json())
        .then(() => {
            getItems();
        })
        .catch(error => console.error('Unable to add item.', error));
}

function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'order' : 'orders';

    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayItems(data) {
    const tBody = document.getElementById('sodaOrders');
    tBody.innerHTML = '';

    _displayCount(data.length);

    data.forEach(item => {
        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let orderId = document.createTextNode(item.id);
        td1.appendChild(orderId);

        let td2 = tr.insertCell(1);
        let sodaName = document.createTextNode(item.soda);
        td2.appendChild(sodaName);

        let td3 = tr.insertCell(2);
        let retrivalPin = document.createTextNode(item.pinCode);
        td3.appendChild(retrivalPin);

        let td4 = tr.insertCell(3);
        let status = document.createTextNode(item.isComplete == true ? 'Claimed' : 'Unclaimed');
        td4.setAttribute('class', item.isComplete == true ? 'success' : 'warning')
        td4.appendChild(status);
    });

    sodaOrders = data;
}
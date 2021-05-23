// Определяем собственный фильтр валют "currency". 
Vue.filter('currency', function (value) {
    return '$' + value.toFixed(2);
});

var demo = new Vue({
    el: '#main',
    data: {
        // Определяем свойства модели, представление будет проходить циклом
        // по массиву услуг и генерировать элементы списка
        // для каждого вложенного пункта.
        services: [
            {
                name: 'Веб разработка',
                price: 300,
                active: true
            }, {
                name: 'Дизайн',
                price: 400,
                active: false
            }, {
                name: 'Интеграция',
                price: 250,
                active: false
            }, {
                name: 'Обучение',
                price: 220,
                active: false
            }
        ]
    },
    methods: {
        toggleActive: function (s) {
            s.active = !s.active;
        },
        total: function () {

            var total = 0;

            this.services.forEach(function (s) {
                if (s.active) {
                    total += s.price;
                }
            });

            return total;
        }
    }
});
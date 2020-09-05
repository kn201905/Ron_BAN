# このメッセージを作った意図
* デュラチャで毎回同じ説明をするのが大変なため。
* このページにあるコードは、デュラチャの荒らし対策のためのコード。

# このリポジトリにあるコードを作成したきっかけ
* 最初は荒らし対策のため、javascriptで簡単な対策コードを作成し、ブラウザの拡張機能で対応した。
* 荒らしは拡張機能で対策をとられたことに気づき、以下のような嫌がらせを始めた。<br>
javascript の反応速度を上回る速度で入退室を繰り返すという手法。<br>
![ron_arashi](https://github.com/kn201905/Ron_BAN/blob/master/ron.jpg)
* javascript では対応できないため、このページにある C# で作成したコードで対応することになった。

# このコードはどういう動作をする？
　荒らしの ip 情報を蓄積していき、荒らしの ip を検知次第、即 BAN するという単純なもの。（荒らしが入室すると、平均１秒程度で BAN 可能）<br>
　私が対応したロンという荒らしは、プロキシを利用して次々に ip 情報を別のものに差し替えて嫌がらせをするというものであった。しかし、こちらはワンクリックで BAN が可能で、一度 BAN された ip は利用することができないため、プロキシを新しく見つけてくるコストに見合わなくなったらしくロンが私の部屋に来ることはほとんどなくなった。<br>
　以上のことにより、このような簡単な対策で荒らしに十分対応をとれるものと判断できる。
 
# 荒らしに対して私がどう思っているか
* インターネットの世界では、上記のロンみたいな荒らしみたいな人がいるのは当然。荒らすために連投してる様子も「にぎやかな花火」を見る感覚で見ている。
* しかし、javascript での対策を上回ることをされると技術者としては腕がうずくw
* 匿名と言えど、好き勝手なことばかりをするとお灸を据えられる、ということを荒らしは知るべきである。<br>
　荒らしは翌日などに部屋に入ってきて会話をしようとしてくるが、一度登録された ip で入ってくると１秒程度で BAN されるため、会話をすることができない。<br>
　今までは荒らしであっても、まあいいか、ということで会話をすることを許していたが、このアプリを利用するようになって自動的に BAN されることとなった。

# このコードを公開してる意図は？
　私の本職は C++ であるが、久しぶりに C# をいじってみたくて C# でコードを組んでみた。<br>
　すると、意外にも初学者の参考になるようなコードができあがった。特筆すべきは以下の２点。以下のことに興味がある人にとっては、大変参考になるコードとなったため、広く公開することにした。
* 通信系のコードの書き方（低レイヤである socket を用いたコードと、http プロキシを利用するときの http ヘッダの書き方も参考にあげている。）
* C# 6.0以降（Roslyn以降）での非同期メカニズムを本格的に活用している。<br>
　ネットに上がっている C# の非同期メカニズムについては、C#5.0 以前のものが多く、6.0以降の記事がほとんど見当たらなかった。

# 今後どうしたいか？
　このページのコードはかなり良い仕上がりになったと思う。できればこのコードをもっと面白いものにしたいけれど、さすがに遊びのコードを書き続ける訳にはいかず、私には時間がないため、このコードの続きを書いてみたい、という人がいたら大変ありがたい。

# 注意
　このページにあるコードは UI 関連は省いているため、このページのコードを実際に動作させたい場合は UI を自分で実装してほしい。通信系のコードや、非同期メカニズムを学習したい人は Drrr_Host2.cs、HttpTask.cs を参考にしてほしい。これらのコードは UI に関係なく動作するように設計してあるため、この２つだけを読んでみてもいいと思う。
